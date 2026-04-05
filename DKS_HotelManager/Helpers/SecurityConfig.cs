using System;
using System.Configuration;

namespace DKS_HotelManager.Helpers
{
    public static class SecurityConfig
    {
        private static readonly string[] PlaceholderValues =
        {
            "__SET_IN_ENV__",
            "__REDACTED__",
            "CHANGE_ME"
        };

        public static string GetSecret(string appSettingKey, string envVarName, bool required = true)
        {
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue.Trim();
            }

            var appValue = ConfigurationManager.AppSettings[appSettingKey];
            if (!string.IsNullOrWhiteSpace(appValue) && !IsPlaceholder(appValue))
            {
                return appValue.Trim();
            }

            return required ? null : string.Empty;
        }

        private static bool IsPlaceholder(string value)
        {
            foreach (var placeholder in PlaceholderValues)
            {
                if (string.Equals(value?.Trim(), placeholder, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
