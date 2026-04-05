using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DKS_HotelManager.Helpers
{
    public class RateLimitResult
    {
        public bool IsAllowed { get; set; }
        public bool IsBlocked { get; set; }
        public int RetryAfterSeconds { get; set; }
        public int Remaining { get; set; }
    }

    public static class SecurityRateLimiter
    {
        private class ThrottleState
        {
            public Queue<DateTime> Attempts { get; } = new Queue<DateTime>();
            public DateTime? BlockedUntilUtc { get; set; }
        }

        private static readonly ConcurrentDictionary<string, ThrottleState> States = new ConcurrentDictionary<string, ThrottleState>();

        public static RateLimitResult CheckAndIncrement(
            string key,
            int maxAttempts,
            TimeSpan window,
            TimeSpan blockDuration)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return new RateLimitResult { IsAllowed = true, Remaining = maxAttempts };
            }

            var state = States.GetOrAdd(key, _ => new ThrottleState());
            var now = DateTime.UtcNow;

            lock (state)
            {
                if (state.BlockedUntilUtc.HasValue && state.BlockedUntilUtc.Value > now)
                {
                    return new RateLimitResult
                    {
                        IsAllowed = false,
                        IsBlocked = true,
                        RetryAfterSeconds = (int)Math.Ceiling((state.BlockedUntilUtc.Value - now).TotalSeconds),
                        Remaining = 0
                    };
                }

                state.BlockedUntilUtc = null;

                while (state.Attempts.Count > 0 && now - state.Attempts.Peek() > window)
                {
                    state.Attempts.Dequeue();
                }

                if (state.Attempts.Count >= maxAttempts)
                {
                    state.BlockedUntilUtc = now.Add(blockDuration);
                    return new RateLimitResult
                    {
                        IsAllowed = false,
                        IsBlocked = true,
                        RetryAfterSeconds = (int)Math.Ceiling(blockDuration.TotalSeconds),
                        Remaining = 0
                    };
                }

                state.Attempts.Enqueue(now);
                return new RateLimitResult
                {
                    IsAllowed = true,
                    IsBlocked = false,
                    RetryAfterSeconds = 0,
                    Remaining = Math.Max(maxAttempts - state.Attempts.Count, 0)
                };
            }
        }

        public static void Reset(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            States.TryRemove(key, out _);
        }
    }
}
