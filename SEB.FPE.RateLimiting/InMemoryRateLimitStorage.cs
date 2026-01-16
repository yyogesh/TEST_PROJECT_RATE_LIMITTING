using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SEB.FPE.RateLimiting
{
    public class InMemoryRateLimitStorage : IRateLimitStorage
    {
        private readonly ConcurrentDictionary<string, InMemoryRateLimitEntry> _cache = new();
        private readonly Timer _cleanupTimer;

        public InMemoryRateLimitStorage()
        {
            // Cleanup expired entries every 5 minutes
            _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public Task<RateLimitInfo> GetRateLimitInfoAsync(string ipAddress, int windowSeconds)
        {
            var windowStart = GetWindowStart(DateTime.UtcNow, windowSeconds);
            var key = GetCacheKey(ipAddress, windowSeconds);
            
            if (_cache.TryGetValue(key, out var entry))
            {
                var now = DateTime.UtcNow;
                var windowEnd = entry.WindowStart.AddSeconds(windowSeconds);

                // If window has expired, reset
                if (now > windowEnd)
                {
                    _cache.TryRemove(key, out _);
                    return Task.FromResult(new RateLimitInfo
                    {
                        RequestCount = 0,
                        WindowStart = windowStart,
                        IsLimitExceeded = false
                    });
                }

                return Task.FromResult(new RateLimitInfo
                {
                    RequestCount = entry.RequestCount,
                    WindowStart = entry.WindowStart,
                    IsLimitExceeded = entry.RequestCount >= entry.PermitLimit
                });
            }

            return Task.FromResult(new RateLimitInfo
            {
                RequestCount = 0,
                WindowStart = windowStart,
                IsLimitExceeded = false
            });
        }

        public Task<bool> IncrementRequestCountAsync(string ipAddress, int windowSeconds, int permitLimit)
        {
            var windowStart = GetWindowStart(DateTime.UtcNow, windowSeconds);
            var key = GetCacheKey(ipAddress, windowSeconds);
            var now = DateTime.UtcNow;

            var entry = _cache.AddOrUpdate(
                key,
                new InMemoryRateLimitEntry
                {
                    RequestCount = 1,
                    WindowStart = windowStart,
                    PermitLimit = permitLimit,
                    WindowSeconds = windowSeconds
                },
                (k, existing) =>
                {
                    var windowEnd = existing.WindowStart.AddSeconds(windowSeconds);
                    
                    // If window expired, reset
                    if (now > windowEnd)
                    {
                        return new InMemoryRateLimitEntry
                        {
                            RequestCount = 1,
                            WindowStart = windowStart,
                            PermitLimit = permitLimit,
                            WindowSeconds = windowSeconds
                        };
                    }

                    // Increment if under limit
                    if (existing.RequestCount < permitLimit)
                    {
                        existing.RequestCount++;
                    }

                    return existing;
                });

            return Task.FromResult(entry.RequestCount <= permitLimit);
        }

        public Task CleanupExpiredEntriesAsync()
        {
            CleanupExpiredEntries(null);
            return Task.CompletedTask;
        }

        private void CleanupExpiredEntries(object state)
        {
            var now = DateTime.UtcNow;
            var keysToRemove = new System.Collections.Generic.List<string>();

            foreach (var kvp in _cache)
            {
                var windowEnd = kvp.Value.WindowStart.AddSeconds(kvp.Value.WindowSeconds);
                if (now > windowEnd)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.TryRemove(key, out _);
            }
        }

        private string GetCacheKey(string ipAddress, int windowSeconds)
        {
            var windowStart = GetWindowStart(DateTime.UtcNow, windowSeconds);
            return $"{ipAddress}:{windowSeconds}:{windowStart:yyyyMMddHHmmss}";
        }

        private DateTime GetWindowStart(DateTime dateTime, int windowSeconds)
        {
            var totalSeconds = (long)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
            var windowNumber = totalSeconds / windowSeconds;
            var windowStartSeconds = windowNumber * windowSeconds;
            return new DateTime(1970, 1, 1).AddSeconds(windowStartSeconds);
        }

        private class InMemoryRateLimitEntry
        {
            public int RequestCount { get; set; }
            public DateTime WindowStart { get; set; }
            public int PermitLimit { get; set; }
            public int WindowSeconds { get; set; }
        }
    }
}
