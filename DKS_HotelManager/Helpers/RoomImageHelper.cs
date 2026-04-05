using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace DKS_HotelManager.Helpers
{
    public static class RoomImageHelper
    {
        private static readonly string[] ImageExtensions = { string.Empty, ".jpg", ".jpeg", ".png", ".webp" };

        public static string ResolveRoomImagePath(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return null;
            }

            var trimmed = rawPath.Trim();
            if (trimmed.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            if (trimmed.StartsWith("~") || trimmed.StartsWith("/"))
            {
                if (FileExists(trimmed))
                {
                    return trimmed;
                }
            }

            var normalized = trimmed;
            if (normalized.StartsWith("~/RoomImages/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring("~/RoomImages/".Length);
            }

            var candidateNames = BuildCandidateNames(normalized);
            foreach (var extension in ImageExtensions)
            {
                foreach (var candidate in candidateNames)
                {
                    var candidateName = candidate.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                        ? candidate
                        : candidate + extension;

                    var roomRelative = $"~/RoomImages/{candidateName.TrimStart('~', '/', '\\')}";
                    if (FileExists(roomRelative))
                    {
                        return roomRelative;
                    }
                }
            }

            foreach (var candidate in candidateNames)
            {
                foreach (var extension in ImageExtensions)
                {
                    var candidateName = candidate.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                        ? candidate
                        : candidate + extension;

                    var fallback = $"~/Images/{candidateName.TrimStart('~', '/', '\\')}";
                    if (FileExists(fallback))
                    {
                        return fallback;
                    }
                }
            }

            return null;
        }

        private static IEnumerable<string> BuildCandidateNames(string baseName)
        {
            var sanitized = baseName.TrimStart('~', '/', '\\').Trim();
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                yield break;
            }

            var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                sanitized,
                sanitized.ToLowerInvariant(),
                sanitized.ToUpperInvariant()
            };

            foreach (var candidate in candidates)
            {
                yield return candidate;
            }
        }

        private static bool FileExists(string virtualPath)
        {
            try
            {
                var physical = HostingEnvironment.MapPath(virtualPath);
                return physical != null && File.Exists(physical);
            }
            catch
            {
                return false;
            }
        }
    }
}
