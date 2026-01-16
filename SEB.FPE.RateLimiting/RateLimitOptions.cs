using System;

namespace SEB.FPE.RateLimiting
{
    public class RateLimitOptions
    {
        /// <summary>
        /// Maximum number of requests allowed per time window
        /// </summary>
        public int PermitLimit { get; set; } = 100;

        /// <summary>
        /// Time window in seconds
        /// </summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// Storage type: "InMemory" or "SqlServer"
        /// </summary>
        public string StorageType { get; set; } = "InMemory";

        /// <summary>
        /// Whether rate limiting is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Paths to exclude from rate limiting (comma-separated)
        /// </summary>
        public string ExcludedPaths { get; set; } = "/health,/swagger";

        /// <summary>
        /// HTTP status code to return when rate limit is exceeded
        /// </summary>
        public int StatusCode { get; set; } = 429;

        /// <summary>
        /// Message to return when rate limit is exceeded
        /// </summary>
        public string Message { get; set; } = "Rate limit exceeded. Please try again later.";
    }
}
