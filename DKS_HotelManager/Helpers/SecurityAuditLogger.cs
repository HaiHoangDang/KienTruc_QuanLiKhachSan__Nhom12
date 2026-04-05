using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Newtonsoft.Json;

namespace DKS_HotelManager.Helpers
{
    public static class SecurityAuditLogger
    {
        private static readonly object FileLock = new object();

        public static void Log(string category, string action, string severity, IDictionary<string, object> details = null)
        {
            try
            {
                var payload = new Dictionary<string, object>
                {
                    { "timestampUtc", DateTime.UtcNow.ToString("o") },
                    { "category", category ?? "security" },
                    { "action", action ?? "unknown" },
                    { "severity", severity ?? "info" }
                };

                if (details != null)
                {
                    foreach (var item in details)
                    {
                        payload[item.Key] = item.Value;
                    }
                }

                var line = JsonConvert.SerializeObject(payload);
                var path = ResolveLogPath();
                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                lock (FileLock)
                {
                    File.AppendAllText(path, line + Environment.NewLine);
                }
            }
            catch
            {
                // Logging must never break request flow.
            }
        }

        public static string GetClientIp(HttpRequestBase request)
        {
            if (request == null)
            {
                return "unknown";
            }

            var ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrWhiteSpace(ip))
            {
                return ip.Split(',')[0].Trim();
            }

            ip = request.UserHostAddress;
            return string.IsNullOrWhiteSpace(ip) ? "unknown" : ip.Trim();
        }

        private static string ResolveLogPath()
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                return null;
            }

            return context.Server.MapPath("~/App_Data/security-audit.log");
        }
    }
}
