using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PrakashCRM.Security
{
    public static class TrustedDeviceStore
    {
        private const string DatabaseName = "TrustedDevices";
        private static readonly object InitializationLock = new object();
        private static bool _initialized;

        public static void EnsureCreated()
        {
            if (_initialized)
                return;

            lock (InitializationLock)
            {
                if (_initialized)
                    return;

                AppDomain.CurrentDomain.SetData("DataDirectory", GetAppDataPath());
                EnsureDatabase();
                EnsureTable();
                _initialized = true;
            }
        }

        public static TrustedDeviceRecord IssueToken(string userNo, string fingerprint, int expiryDays)
        {
            EnsureCreated();

            if (string.IsNullOrWhiteSpace(userNo) || string.IsNullOrWhiteSpace(fingerprint) || expiryDays <= 0)
                return null;

            DateTime utcNow = DateTime.UtcNow;
            string fingerprintHash = ComputeFingerprintHash(fingerprint);
            RevokeActiveTokens(userNo.Trim(), fingerprintHash, utcNow);

            TrustedDeviceRecord record = new TrustedDeviceRecord
            {
                UserNo = userNo.Trim(),
                Token = GenerateSecureToken(),
                FingerprintHash = fingerprintHash,
                ExpiryUtc = utcNow.AddDays(expiryDays),
                CreatedUtc = utcNow,
                LastUsedUtc = utcNow,
                IsRevoked = false
            };

            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
INSERT INTO dbo.TrustedDevices (UserNo, Token, FingerprintHash, ExpiryUtc, CreatedUtc, LastUsedUtc, IsRevoked)
OUTPUT INSERTED.Id
VALUES (@UserNo, @Token, @FingerprintHash, @ExpiryUtc, @CreatedUtc, @LastUsedUtc, 0);";
                command.Parameters.AddWithValue("@UserNo", record.UserNo);
                command.Parameters.AddWithValue("@Token", record.Token);
                command.Parameters.AddWithValue("@FingerprintHash", record.FingerprintHash);
                command.Parameters.AddWithValue("@ExpiryUtc", record.ExpiryUtc);
                command.Parameters.AddWithValue("@CreatedUtc", record.CreatedUtc);
                command.Parameters.AddWithValue("@LastUsedUtc", record.LastUsedUtc);

                connection.Open();
                record.Id = Convert.ToInt32(command.ExecuteScalar());
            }

            return record;
        }

        public static TrustedDeviceValidationResult ValidateAndRenewToken(string userNo, string cookieToken, string storageToken, string fingerprint, int expiryDays)
        {
            EnsureCreated();

            TrustedDeviceValidationResult invalidResult = new TrustedDeviceValidationResult
            {
                IsValid = false,
                ShouldClearClientState = !string.IsNullOrWhiteSpace(cookieToken) || !string.IsNullOrWhiteSpace(storageToken)
            };

            if (string.IsNullOrWhiteSpace(userNo)
                || string.IsNullOrWhiteSpace(cookieToken)
                || string.IsNullOrWhiteSpace(storageToken)
                || string.IsNullOrWhiteSpace(fingerprint)
                || expiryDays <= 0)
            {
                return invalidResult;
            }

            cookieToken = cookieToken.Trim();
            storageToken = storageToken.Trim();

            if (!string.Equals(cookieToken, storageToken, StringComparison.Ordinal))
                return invalidResult;

            TrustedDeviceRecord record = GetToken(cookieToken);
            if (record == null || record.IsRevoked)
                return invalidResult;

            DateTime utcNow = DateTime.UtcNow;
            if (record.ExpiryUtc <= utcNow)
            {
                RevokeToken(record.Id, utcNow);
                return invalidResult;
            }

            if (!string.Equals(record.UserNo ?? string.Empty, userNo.Trim(), StringComparison.OrdinalIgnoreCase))
                return invalidResult;

            if (!string.Equals(record.FingerprintHash ?? string.Empty, ComputeFingerprintHash(fingerprint), StringComparison.Ordinal))
                return invalidResult;

            DateTime newExpiryUtc = utcNow.AddDays(expiryDays);
            TrustedDeviceRecord renewedRecord = RenewToken(record.Id, newExpiryUtc, utcNow);
            if (renewedRecord == null)
                return invalidResult;

            return new TrustedDeviceValidationResult
            {
                IsValid = true,
                ShouldClearClientState = false,
                Record = renewedRecord
            };
        }

        public static void InvalidateAllDevicesForUser(string userNo)
        {
            EnsureCreated();

            if (string.IsNullOrWhiteSpace(userNo))
                return;

            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
UPDATE dbo.TrustedDevices
SET IsRevoked = 1,
    RevokedUtc = @RevokedUtc
WHERE UserNo = @UserNo
  AND IsRevoked = 0;";
                command.Parameters.AddWithValue("@UserNo", userNo.Trim());
                command.Parameters.AddWithValue("@RevokedUtc", DateTime.UtcNow);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static TrustedDeviceRecord RenewToken(int id, DateTime expiryUtc, DateTime lastUsedUtc)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
UPDATE dbo.TrustedDevices
SET ExpiryUtc = @ExpiryUtc,
    LastUsedUtc = @LastUsedUtc
OUTPUT INSERTED.Id, INSERTED.UserNo, INSERTED.Token, INSERTED.FingerprintHash, INSERTED.ExpiryUtc,
       INSERTED.CreatedUtc, INSERTED.LastUsedUtc, INSERTED.IsRevoked, INSERTED.RevokedUtc
WHERE Id = @Id
  AND IsRevoked = 0;";
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@ExpiryUtc", expiryUtc);
                command.Parameters.AddWithValue("@LastUsedUtc", lastUsedUtc);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                        return null;

                    return MapRecord(reader);
                }
            }
        }

        private static void RevokeActiveTokens(string userNo, string fingerprintHash, DateTime revokedUtc)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
UPDATE dbo.TrustedDevices
SET IsRevoked = 1,
    RevokedUtc = @RevokedUtc
WHERE UserNo = @UserNo
  AND FingerprintHash = @FingerprintHash
  AND IsRevoked = 0;";
                command.Parameters.AddWithValue("@UserNo", userNo);
                command.Parameters.AddWithValue("@FingerprintHash", fingerprintHash);
                command.Parameters.AddWithValue("@RevokedUtc", revokedUtc);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void RevokeToken(int id, DateTime revokedUtc)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
UPDATE dbo.TrustedDevices
SET IsRevoked = 1,
    RevokedUtc = @RevokedUtc
WHERE Id = @Id
  AND IsRevoked = 0;";
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@RevokedUtc", revokedUtc);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static TrustedDeviceRecord GetToken(string token)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP (1) Id, UserNo, Token, FingerprintHash, ExpiryUtc, CreatedUtc, LastUsedUtc, IsRevoked, RevokedUtc
FROM dbo.TrustedDevices
WHERE Token = @Token
ORDER BY Id DESC;";
                command.Parameters.AddWithValue("@Token", token);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                        return null;

                    return MapRecord(reader);
                }
            }
        }

        private static TrustedDeviceRecord MapRecord(SqlDataReader reader)
        {
            return new TrustedDeviceRecord
            {
                Id = reader.GetInt32(0),
                UserNo = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                Token = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                FingerprintHash = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                ExpiryUtc = reader.GetDateTime(4),
                CreatedUtc = reader.GetDateTime(5),
                LastUsedUtc = reader.GetDateTime(6),
                IsRevoked = reader.GetBoolean(7),
                RevokedUtc = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
            };
        }

        private static string ComputeFingerprintHash(string fingerprint)
        {
            fingerprint = (fingerprint ?? string.Empty).Trim();
            byte[] inputBytes = Encoding.UTF8.GetBytes(fingerprint);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                StringBuilder builder = new StringBuilder(hashBytes.Length * 2);

                for (int index = 0; index < hashBytes.Length; index++)
                    builder.Append(hashBytes[index].ToString("x2"));

                return builder.ToString();
            }
        }

        private static string GenerateSecureToken(int sizeInBytes = 32)
        {
            byte[] buffer = new byte[sizeInBytes];

            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(buffer);
            }

            return Convert.ToBase64String(buffer)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static void EnsureDatabase()
        {
            string dbFilePath = GetDatabaseFilePath().Replace("'", "''");
            string logFilePath = GetDatabaseLogFilePath().Replace("'", "''");

            string sql = @"
IF DB_ID(N'TrustedDevices') IS NULL
BEGIN
    CREATE DATABASE [TrustedDevices]
    ON PRIMARY (NAME = N'TrustedDevices', FILENAME = N'" + dbFilePath + @"')
    LOG ON (NAME = N'TrustedDevices_log', FILENAME = N'" + logFilePath + @"');
END";

            using (SqlConnection connection = new SqlConnection(GetMasterConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void EnsureTable()
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
IF OBJECT_ID(N'dbo.TrustedDevices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrustedDevices
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserNo NVARCHAR(50) NOT NULL,
        Token NVARCHAR(256) NOT NULL,
        FingerprintHash NVARCHAR(128) NOT NULL,
        ExpiryUtc DATETIME NOT NULL,
        CreatedUtc DATETIME NOT NULL,
        LastUsedUtc DATETIME NOT NULL,
        IsRevoked BIT NOT NULL CONSTRAINT DF_TrustedDevices_IsRevoked DEFAULT(0),
        RevokedUtc DATETIME NULL
    );

    CREATE UNIQUE INDEX IX_TrustedDevices_Token ON dbo.TrustedDevices(Token);
    CREATE INDEX IX_TrustedDevices_UserNo ON dbo.TrustedDevices(UserNo);
END";

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static string GetMasterConnectionString()
        {
            return @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;";
        }

        private static string GetConnectionString()
        {
            return @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\TrustedDevices.mdf;Initial Catalog=" + DatabaseName + ";Integrated Security=True;Connect Timeout=30;";
        }

        private static string GetAppDataPath()
        {
            string appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            return appDataPath;
        }

        private static string GetDatabaseFilePath()
        {
            return Path.Combine(GetAppDataPath(), DatabaseName + ".mdf");
        }

        private static string GetDatabaseLogFilePath()
        {
            return Path.Combine(GetAppDataPath(), DatabaseName + "_log.ldf");
        }
    }

    public sealed class TrustedDeviceRecord
    {
        public int Id { get; set; }

        public string UserNo { get; set; }

        public string Token { get; set; }

        public string FingerprintHash { get; set; }

        public DateTime ExpiryUtc { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime LastUsedUtc { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime? RevokedUtc { get; set; }
    }

    public sealed class TrustedDeviceValidationResult
    {
        public bool IsValid { get; set; }

        public bool ShouldClearClientState { get; set; }

        public TrustedDeviceRecord Record { get; set; }
    }
}
