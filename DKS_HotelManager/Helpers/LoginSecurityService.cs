using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DKS_HotelManager.Helpers
{
    public class LoginAttemptResult
    {
        public bool Allowed { get; set; }
        public int RetryAfterSeconds { get; set; }
    }

    public static class LoginSecurityService
    {
        private class FailureState
        {
            public Queue<DateTime> FailedAttempts { get; } = new Queue<DateTime>();
            public DateTime? BlockedUntilUtc { get; set; }
        }

        private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private const int MaxFailedAttempts = 5;
        private static readonly ConcurrentDictionary<string, FailureState> FailureStates = new ConcurrentDictionary<string, FailureState>();

        public static LoginAttemptResult CheckLoginAllowed(string username, string ipAddress)
        {
            var key = BuildKey(username, ipAddress);
            var state = FailureStates.GetOrAdd(key, _ => new FailureState());
            var now = DateTime.UtcNow;

            lock (state)
            {
                if (state.BlockedUntilUtc.HasValue && state.BlockedUntilUtc.Value > now)
                {
                    return new LoginAttemptResult
                    {
                        Allowed = false,
                        RetryAfterSeconds = (int)Math.Ceiling((state.BlockedUntilUtc.Value - now).TotalSeconds)
                    };
                }

                if (state.BlockedUntilUtc.HasValue && state.BlockedUntilUtc.Value <= now)
                {
                    state.BlockedUntilUtc = null;
                }

                while (state.FailedAttempts.Count > 0 && now - state.FailedAttempts.Peek() > AttemptWindow)
                {
                    state.FailedAttempts.Dequeue();
                }

                return new LoginAttemptResult
                {
                    Allowed = true,
                    RetryAfterSeconds = 0
                };
            }
        }

        public static LoginAttemptResult RecordFailure(string username, string ipAddress)
        {
            var key = BuildKey(username, ipAddress);
            var state = FailureStates.GetOrAdd(key, _ => new FailureState());
            var now = DateTime.UtcNow;

            lock (state)
            {
                while (state.FailedAttempts.Count > 0 && now - state.FailedAttempts.Peek() > AttemptWindow)
                {
                    state.FailedAttempts.Dequeue();
                }

                state.FailedAttempts.Enqueue(now);

                if (state.FailedAttempts.Count >= MaxFailedAttempts)
                {
                    state.BlockedUntilUtc = now.Add(LockoutDuration);
                    state.FailedAttempts.Clear();
                    return new LoginAttemptResult
                    {
                        Allowed = false,
                        RetryAfterSeconds = (int)Math.Ceiling(LockoutDuration.TotalSeconds)
                    };
                }
            }

            return new LoginAttemptResult { Allowed = true, RetryAfterSeconds = 0 };
        }

        public static void ResetFailures(string username, string ipAddress)
        {
            FailureStates.TryRemove(BuildKey(username, ipAddress), out _);
        }

        private static string BuildKey(string username, string ipAddress)
        {
            var safeUser = string.IsNullOrWhiteSpace(username) ? "unknown-user" : username.Trim().ToLowerInvariant();
            var safeIp = string.IsNullOrWhiteSpace(ipAddress) ? "unknown-ip" : ipAddress.Trim();
            return $"login:{safeUser}:{safeIp}";
        }
    }
}
