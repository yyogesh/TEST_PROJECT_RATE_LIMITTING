using System;
using System.Threading.Tasks;

namespace SEB.FPE.RateLimiting
{
    public interface IRateLimitStorage
    {
        /// <summary>
        /// Gets the current request count for an IP address
        /// </summary>
        Task<RateLimitInfo> GetRateLimitInfoAsync(string ipAddress, int windowSeconds);

        /// <summary>
        /// Increments the request count for an IP address
        /// </summary>
        Task<bool> IncrementRequestCountAsync(string ipAddress, int windowSeconds, int permitLimit);

        /// <summary>
        /// Cleans up expired entries (optional, for maintenance)
        /// </summary>
        Task CleanupExpiredEntriesAsync();
    }

    public class RateLimitInfo
    {
        public int RequestCount { get; set; }
        public DateTime WindowStart { get; set; }
        public bool IsLimitExceeded { get; set; }
    }
}
