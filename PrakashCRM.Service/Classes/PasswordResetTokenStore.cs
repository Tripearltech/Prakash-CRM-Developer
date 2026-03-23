using PrakashCRM.Data.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace PrakashCRM.Service.Classes
{
    public static class PasswordResetTokenStore
    {
        private const string DatabaseName = "PasswordResetTokens";
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

        public static PasswordResetTokenRecord CreateToken(string userNo, string email, string role, int expiryMinutes)
        {
            EnsureCreated();

            DateTime utcNow = DateTime.UtcNow;
            InvalidateActiveTokens(userNo, email, utcNow);

            PasswordResetTokenRecord record = new PasswordResetTokenRecord
            {
                UserNo = userNo,
                Email = email,
                Role = role,
                Token = PasswordSecurity.GenerateSecureToken(),
                ExpiryUtc = utcNow.AddMinutes(expiryMinutes),
                IsUsed = false,
                CreatedUtc = utcNow
            };

            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
INSERT INTO dbo.PasswordResetTokens (UserNo, Email, Role, Token, ExpiryUtc, IsUsed, CreatedUtc)
OUTPUT INSERTED.Id
VALUES (@UserNo, @Email, @Role, @Token, @ExpiryUtc, 0, @CreatedUtc);";
                command.Parameters.AddWithValue("@UserNo", (object)record.UserNo ?? DBNull.Value);
                command.Parameters.AddWithValue("@Email", (object)record.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@Role", (object)record.Role ?? DBNull.Value);
                command.Parameters.AddWithValue("@Token", record.Token);
                command.Parameters.AddWithValue("@ExpiryUtc", record.ExpiryUtc);
                command.Parameters.AddWithValue("@CreatedUtc", record.CreatedUtc);

                connection.Open();
                record.Id = Convert.ToInt32(command.ExecuteScalar());
            }

            return record;
        }

        public static PasswordResetValidationResult ValidateToken(string token)
        {
            EnsureCreated();

            if (string.IsNullOrWhiteSpace(token))
                return BuildValidationResult(false, "Invalid", "Invalid link");

            PasswordResetTokenRecord record = GetToken(token.Trim());
            if (record == null)
                return BuildValidationResult(false, "Invalid", "Invalid link");

            if (record.IsUsed)
                return BuildValidationResult(false, "Used", "Link already used", record);

            if (record.ExpiryUtc <= DateTime.UtcNow)
                return BuildValidationResult(false, "Expired", "Link expired", record);

            return BuildValidationResult(true, "Valid", string.Empty, record);
        }

        public static PasswordResetTokenRecord TryMarkTokenUsed(string token, DateTime usedUtc)
        {
            EnsureCreated();

            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
UPDATE dbo.PasswordResetTokens
SET IsUsed = 1,
    UsedUtc = @UsedUtc
OUTPUT INSERTED.Id, INSERTED.UserNo, INSERTED.Email, INSERTED.Role, INSERTED.Token, INSERTED.ExpiryUtc, INSERTED.IsUsed, INSERTED.CreatedUtc, INSERTED.UsedUtc
WHERE Token = @Token
  AND IsUsed = 0
  AND ExpiryUtc > @UsedUtc;";
                command.Parameters.AddWithValue("@Token", token);
                command.Parameters.AddWithValue("@UsedUtc", usedUtc);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return MapRecord(reader);
                }
            }
        }

        public static void RevertTokenUsage(int tokenId)
        {
            EnsureCreated();

            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
UPDATE dbo.PasswordResetTokens
SET IsUsed = 0,
    UsedUtc = NULL
WHERE Id = @Id;";
                command.Parameters.AddWithValue("@Id", tokenId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void InvalidateActiveTokens(string userNo, string email, DateTime usedUtc)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
UPDATE dbo.PasswordResetTokens
SET IsUsed = 1,
    UsedUtc = @UsedUtc
WHERE IsUsed = 0
  AND (UserNo = @UserNo OR Email = @Email);";
                command.Parameters.AddWithValue("@UsedUtc", usedUtc);
                command.Parameters.AddWithValue("@UserNo", (object)userNo ?? DBNull.Value);
                command.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static PasswordResetTokenRecord GetToken(string token)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP (1) Id, UserNo, Email, Role, Token, ExpiryUtc, IsUsed, CreatedUtc, UsedUtc
FROM dbo.PasswordResetTokens
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

        private static PasswordResetValidationResult BuildValidationResult(bool isValid, string status, string message, PasswordResetTokenRecord record = null)
        {
            return new PasswordResetValidationResult
            {
                IsValid = isValid,
                Status = status,
                Message = message,
                Token = record != null ? record.Token : null,
                UserNo = record != null ? record.UserNo : null,
                Email = record != null ? record.Email : null,
                Role = record != null ? record.Role : null
            };
        }

        private static PasswordResetTokenRecord MapRecord(SqlDataReader reader)
        {
            return new PasswordResetTokenRecord
            {
                Id = reader.GetInt32(0),
                UserNo = reader.IsDBNull(1) ? null : reader.GetString(1),
                Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                Role = reader.IsDBNull(3) ? null : reader.GetString(3),
                Token = reader.IsDBNull(4) ? null : reader.GetString(4),
                ExpiryUtc = reader.GetDateTime(5),
                IsUsed = reader.GetBoolean(6),
                CreatedUtc = reader.GetDateTime(7),
                UsedUtc = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
            };
        }

        private static void EnsureDatabase()
        {
            string dbFilePath = GetDatabaseFilePath().Replace("'", "''");
            string logFilePath = GetDatabaseLogFilePath().Replace("'", "''");

            string sql = @"
IF DB_ID(N'PasswordResetTokens') IS NULL
BEGIN
    CREATE DATABASE [PasswordResetTokens]
    ON PRIMARY (NAME = N'PasswordResetTokens', FILENAME = N'" + dbFilePath + @"')
    LOG ON (NAME = N'PasswordResetTokens_log', FILENAME = N'" + logFilePath + @"');
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
IF OBJECT_ID(N'dbo.PasswordResetTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetTokens
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserNo NVARCHAR(50) NULL,
        Email NVARCHAR(256) NULL,
        Role NVARCHAR(100) NULL,
        Token NVARCHAR(256) NOT NULL,
        ExpiryUtc DATETIME NOT NULL,
        IsUsed BIT NOT NULL CONSTRAINT DF_PasswordResetTokens_IsUsed DEFAULT(0),
        CreatedUtc DATETIME NOT NULL,
        UsedUtc DATETIME NULL
    );

    CREATE UNIQUE INDEX IX_PasswordResetTokens_Token ON dbo.PasswordResetTokens(Token);
END";

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static string GetAppDataPath()
        {
            string appDataPath = HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath("~/App_Data")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");

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

        private static string GetMasterConnectionString()
        {
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["PasswordResetTokenDbMaster"];
            return setting != null
                ? setting.ConnectionString
                : @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;MultipleActiveResultSets=True";
        }

        private static string GetConnectionString()
        {
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["PasswordResetTokenDb"];
            return setting != null
                ? setting.ConnectionString
                : @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\PasswordResetTokens.mdf;Initial Catalog=PasswordResetTokens;Integrated Security=True;MultipleActiveResultSets=True";
        }
    }
}