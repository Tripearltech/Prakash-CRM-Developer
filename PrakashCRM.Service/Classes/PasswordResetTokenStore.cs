using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PrakashCRM.Service.Classes
{
    public static class PasswordResetTokenStore
    {
        private const string StoreFileName = "PasswordResetTokens.json";
        private static readonly object InitializationLock = new object();
        private static readonly object StoreLock = new object();
        private static bool _initialized;

        public static void EnsureCreated()
        {
            if (_initialized)
                return;

            lock (InitializationLock)
            {
                if (_initialized)
                    return;

                EnsureStoreFile();
                _initialized = true;
            }
        }

        public static PasswordResetTokenRecord CreateToken(string userNo, string email, string role, int expiryMinutes)
        {
            EnsureCreated();

            lock (StoreLock)
            {
                DateTime utcNow = DateTime.UtcNow;
                List<PasswordResetTokenRecord> records = LoadRecords();

                InvalidateActiveTokens(records, userNo, email, utcNow);
                RemoveExpiredTokens(records, utcNow);

                PasswordResetTokenRecord record = new PasswordResetTokenRecord
                {
                    Id = records.Count == 0 ? 1 : records.Max(item => item.Id) + 1,
                    UserNo = userNo,
                    Email = email,
                    Role = role,
                    Token = PasswordSecurity.GenerateSecureToken(),
                    ExpiryUtc = utcNow.AddMinutes(expiryMinutes),
                    IsUsed = false,
                    CreatedUtc = utcNow
                };

                records.Add(record);
                SaveRecords(records);

                return record;
            }
        }

        public static PasswordResetValidationResult ValidateToken(string token)
        {
            EnsureCreated();

            if (string.IsNullOrWhiteSpace(token))
                return BuildValidationResult(false, "Invalid", "Invalid link");

            lock (StoreLock)
            {
                List<PasswordResetTokenRecord> records = LoadRecords();
                PasswordResetTokenRecord record = GetToken(records, token.Trim());
                if (record == null)
                    return BuildValidationResult(false, "Invalid", "Invalid link");

                if (record.IsUsed)
                    return BuildValidationResult(false, "Used", "Link already used", record);

                if (record.ExpiryUtc <= DateTime.UtcNow)
                    return BuildValidationResult(false, "Expired", "Link expired", record);

                return BuildValidationResult(true, "Valid", string.Empty, record);
            }
        }

        public static PasswordResetTokenRecord TryMarkTokenUsed(string token, DateTime usedUtc)
        {
            EnsureCreated();

            lock (StoreLock)
            {
                List<PasswordResetTokenRecord> records = LoadRecords();
                PasswordResetTokenRecord record = FindTokenRecord(records, token);
                if (record == null || record.IsUsed || record.ExpiryUtc <= usedUtc)
                    return null;

                record.IsUsed = true;
                record.UsedUtc = usedUtc;
                SaveRecords(records);

                return CloneRecord(record);
            }
        }

        public static void RevertTokenUsage(int tokenId)
        {
            EnsureCreated();

            lock (StoreLock)
            {
                List<PasswordResetTokenRecord> records = LoadRecords();
                PasswordResetTokenRecord record = records.FirstOrDefault(item => item.Id == tokenId);
                if (record == null)
                    return;

                record.IsUsed = false;
                record.UsedUtc = null;
                SaveRecords(records);
            }
        }

        private static void InvalidateActiveTokens(List<PasswordResetTokenRecord> records, string userNo, string email, DateTime usedUtc)
        {
            foreach (PasswordResetTokenRecord record in records)
            {
                if (record.IsUsed)
                    continue;

                bool sameUser = string.Equals(record.UserNo, userNo, StringComparison.OrdinalIgnoreCase);
                bool sameEmail = string.Equals(record.Email, email, StringComparison.OrdinalIgnoreCase);
                if (!sameUser && !sameEmail)
                    continue;

                record.IsUsed = true;
                record.UsedUtc = usedUtc;
            }
        }

        private static PasswordResetTokenRecord GetToken(List<PasswordResetTokenRecord> records, string token)
        {
            return CloneRecord(FindTokenRecord(records, token));
        }

        private static PasswordResetTokenRecord FindTokenRecord(List<PasswordResetTokenRecord> records, string token)
        {
            return records
                .Where(item => string.Equals(item.Token, token, StringComparison.Ordinal))
                .OrderByDescending(item => item.Id)
                .FirstOrDefault();
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

        private static PasswordResetTokenRecord CloneRecord(PasswordResetTokenRecord record)
        {
            if (record == null)
                return null;

            return new PasswordResetTokenRecord
            {
                Id = record.Id,
                UserNo = record.UserNo,
                Email = record.Email,
                Role = record.Role,
                Token = record.Token,
                ExpiryUtc = record.ExpiryUtc,
                IsUsed = record.IsUsed,
                CreatedUtc = record.CreatedUtc,
                UsedUtc = record.UsedUtc
            };
        }

        private static void EnsureStoreFile()
        {
            string storeFilePath = GetStoreFilePath();
            if (!File.Exists(storeFilePath))
                File.WriteAllText(storeFilePath, "[]");
        }

        private static List<PasswordResetTokenRecord> LoadRecords()
        {
            string storeFilePath = GetStoreFilePath();
            if (!File.Exists(storeFilePath))
                return new List<PasswordResetTokenRecord>();

            string content = File.ReadAllText(storeFilePath);
            if (string.IsNullOrWhiteSpace(content))
                return new List<PasswordResetTokenRecord>();

            List<PasswordResetTokenRecord> records = JsonConvert.DeserializeObject<List<PasswordResetTokenRecord>>(content);
            return records ?? new List<PasswordResetTokenRecord>();
        }

        private static void SaveRecords(List<PasswordResetTokenRecord> records)
        {
            string storeFilePath = GetStoreFilePath();
            string content = JsonConvert.SerializeObject(records, Formatting.Indented);
            File.WriteAllText(storeFilePath, content);
        }

        private static void RemoveExpiredTokens(List<PasswordResetTokenRecord> records, DateTime utcNow)
        {
            records.RemoveAll(record => record.ExpiryUtc <= utcNow.AddDays(-7));
        }

        private static string GetAppDataPath()
        {
            string appDataPath = HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath("~/App_Data")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");

            Directory.CreateDirectory(appDataPath);
            return appDataPath;
        }

        private static string GetStoreFilePath()
        {
            return Path.Combine(GetAppDataPath(), StoreFileName);
        }
    }
}