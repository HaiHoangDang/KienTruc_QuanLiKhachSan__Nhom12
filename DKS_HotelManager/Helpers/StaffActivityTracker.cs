using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace DKS_HotelManager.Helpers
{
    public static class StaffActivityTracker
    {
        private static readonly ReaderWriterLockSlim FileLock = new ReaderWriterLockSlim();
        private static readonly string LogFilePath = PrepareLogFile();

        public static IEnumerable<StaffActivityRecord> GetRecent(int maxRecords = 12)
        {
            FileLock.EnterReadLock();
            try
            {
                return ReadAll()
                    .OrderByDescending(r => r.Timestamp)
                    .Take(maxRecords)
                    .ToList();
            }
            finally
            {
                FileLock.ExitReadLock();
            }
        }

        public static void RecordEvent(string action, int? staffId = null, string staffName = null, int? hotelId = null, string hotelName = null, string details = null)
        {
            var record = new StaffActivityRecord
            {
                Timestamp = DateTime.UtcNow,
                Event = action,
                StaffId = staffId,
                StaffName = staffName,
                HotelId = hotelId,
                HotelName = hotelName,
                Details = details
            };

            FileLock.EnterWriteLock();
            try
            {
                var records = ReadAll();
                records.Add(record);
                WriteAll(records);
            }
            finally
            {
                FileLock.ExitWriteLock();
            }
        }

        public static TimeSpan? GetCurrentSessionDuration(int staffId)
        {
            FileLock.EnterReadLock();
            try
            {
                var records = ReadAll()
                    .Where(r => r.StaffId == staffId)
                    .OrderBy(r => r.Timestamp)
                    .ToList();

                if (!records.Any())
                {
                    return null;
                }

                // Tìm lần đăng nhập mới nhất
                var lastLogin = records
                    .LastOrDefault(r => string.Equals(r.Event, "Nhân viên lễ tân đăng nhập", StringComparison.Ordinal));

                if (lastLogin == null)
                {
                    return null;
                }

                // Nếu sau lần đăng nhập đó đã có đăng xuất thì coi như không còn hoạt động
                var hasLogoutAfter =
                    records.Any(r =>
                        r.Timestamp > lastLogin.Timestamp &&
                        string.Equals(r.Event, "Nhân viên lễ tân đăng xuất", StringComparison.Ordinal));

                if (hasLogoutAfter)
                {
                    return null;
                }

                var start = lastLogin.Timestamp;
                if (start == default(DateTime))
                {
                    return null;
                }

                var now = DateTime.UtcNow;

                // Bỏ qua các phiên quá cũ (ví dụ lớn hơn 24h) để tránh dính log lịch sử
                if ((now - start) > TimeSpan.FromHours(24))
                {
                    return null;
                }

                if (now <= start)
                {
                    return null;
                }

                return now - start;
            }
            finally
            {
                FileLock.ExitReadLock();
            }
        }

        private static string PrepareLogFile()
        {
            var basePath = HostingEnvironment.MapPath("~/App_Data")
                ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "App_Data");
            Directory.CreateDirectory(basePath);
            return Path.Combine(basePath, "staff-activity.json");
        }

        private static List<StaffActivityRecord> ReadAll()
        {
            if (!File.Exists(LogFilePath))
            {
                return new List<StaffActivityRecord>();
            }

            var payload = File.ReadAllText(LogFilePath);
            if (string.IsNullOrWhiteSpace(payload))
            {
                return new List<StaffActivityRecord>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<StaffActivityRecord>>(payload) ?? new List<StaffActivityRecord>();
            }
            catch
            {
                return new List<StaffActivityRecord>();
            }
        }

        private static void WriteAll(List<StaffActivityRecord> records)
        {
            File.WriteAllText(LogFilePath, JsonConvert.SerializeObject(records, Formatting.Indented));
        }
    }

    public class StaffActivityRecord
    {
        public int? StaffId { get; set; }
        public string StaffName { get; set; }
        public string Event { get; set; }
        public int? HotelId { get; set; }
        public string HotelName { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
