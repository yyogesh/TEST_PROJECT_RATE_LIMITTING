using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SEB.FPE.RateLimiting
{
    /// <summary>
    /// SQL Server storage implementation for rate limiting.
    /// Requires a DbContext with a DbSet&lt;RateLimitEntry&gt; named RateLimitEntries.
    /// </summary>
    public class SqlServerRateLimitStorage : IRateLimitStorage
    {
        private readonly DbContext _dbContext;

        public SqlServerRateLimitStorage(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<RateLimitInfo> GetRateLimitInfoAsync(string ipAddress, int windowSeconds)
        {
            var now = DateTime.UtcNow;
            var windowStart = GetWindowStart(now, windowSeconds);
            var windowEnd = windowStart.AddSeconds(windowSeconds);

            var dbSet = _dbContext.Set<RateLimitEntry>();
            var entry = await dbSet
                .Where(e => e.IpAddress == ipAddress && 
                           e.WindowStart >= windowStart && 
                           e.WindowStart < windowEnd)
                .OrderByDescending(e => e.WindowStart)
                .FirstOrDefaultAsync();

            if (entry == null)
            {
                return new RateLimitInfo
                {
                    RequestCount = 0,
                    WindowStart = windowStart,
                    IsLimitExceeded = false
                };
            }

            return new RateLimitInfo
            {
                RequestCount = entry.RequestCount,
                WindowStart = entry.WindowStart,
                IsLimitExceeded = entry.RequestCount >= entry.PermitLimit
            };
        }

        public async Task<bool> IncrementRequestCountAsync(string ipAddress, int windowSeconds, int permitLimit)
        {
            var now = DateTime.UtcNow;
            var windowStart = GetWindowStart(now, windowSeconds);
            var windowEnd = windowStart.AddSeconds(windowSeconds);

            var dbSet = _dbContext.Set<RateLimitEntry>();
            var entry = await dbSet
                .Where(e => e.IpAddress == ipAddress && 
                           e.WindowStart >= windowStart && 
                           e.WindowStart < windowEnd)
                .FirstOrDefaultAsync();

            if (entry == null)
            {
                entry = new RateLimitEntry
                {
                    IpAddress = ipAddress,
                    WindowStart = windowStart,
                    RequestCount = 1,
                    PermitLimit = permitLimit,
                    CreatedAt = now
                };
                dbSet.Add(entry);
            }
            else
            {
                if (entry.RequestCount < permitLimit)
                {
                    entry.RequestCount++;
                    entry.LastRequestAt = now;
                }
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                // Return true only if request count is strictly less than permit limit
                // If count equals limit, the request should be blocked
                return entry.RequestCount < permitLimit;
            }
            catch (DbUpdateException)
            {
                // Handle concurrency issues - query again and update
                _dbContext.Entry(entry).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                var existingEntry = await dbSet
                    .Where(e => e.IpAddress == ipAddress && 
                               e.WindowStart >= windowStart && 
                               e.WindowStart < windowEnd)
                    .FirstOrDefaultAsync();

                if (existingEntry != null)
                {
                    if (existingEntry.RequestCount < permitLimit)
                    {
                        existingEntry.RequestCount++;
                        existingEntry.LastRequestAt = now;
                        await _dbContext.SaveChangesAsync();
                    }
                    // Return true only if request count is strictly less than permit limit
                    return existingEntry.RequestCount < permitLimit;
                }
                else
                {
                    // Entry was deleted or doesn't exist, create new one
                    entry = new RateLimitEntry
                    {
                        IpAddress = ipAddress,
                        WindowStart = windowStart,
                        RequestCount = 1,
                        PermitLimit = permitLimit,
                        CreatedAt = now
                    };
                    dbSet.Add(entry);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
            }
        }

        public async Task CleanupExpiredEntriesAsync()
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-24); // Keep entries for 24 hours
            var dbSet = _dbContext.Set<RateLimitEntry>();
            var expiredEntries = await dbSet
                .Where(e => e.WindowStart < cutoffTime)
                .ToListAsync();

            if (expiredEntries.Any())
            {
                dbSet.RemoveRange(expiredEntries);
                await _dbContext.SaveChangesAsync();
            }
        }

        private DateTime GetWindowStart(DateTime dateTime, int windowSeconds)
        {
            var totalSeconds = (long)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
            var windowNumber = totalSeconds / windowSeconds;
            var windowStartSeconds = windowNumber * windowSeconds;
            return new DateTime(1970, 1, 1).AddSeconds(windowStartSeconds);
        }
    }
}
