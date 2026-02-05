using System.Collections.Generic;

namespace SEB.FPE.Telemetry
{
    /// <summary>
    /// Configuration options for telemetry middleware
    /// </summary>
    public class TelemetryOptions
    {
        /// <summary>
        /// Whether telemetry is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Whether to log requests
        /// </summary>
        public bool LogRequests { get; set; } = true;

        /// <summary>
        /// Whether to log responses
        /// </summary>
        public bool LogResponses { get; set; } = true;

        /// <summary>
        /// Whether to log exceptions
        /// </summary>
        public bool LogExceptions { get; set; } = true;

        /// <summary>
        /// Maximum request body size to log (in bytes). Default: 10KB
        /// </summary>
        public int MaxRequestBodySize { get; set; } = 10240;

        /// <summary>
        /// Maximum response body size to log (in bytes). Default: 10KB
        /// </summary>
        public int MaxResponseBodySize { get; set; } = 10240;

        /// <summary>
        /// Paths to exclude from telemetry logging (comma-separated)
        /// </summary>
        public string ExcludedPaths { get; set; } = "/health,/swagger,/ui";

        /// <summary>
        /// HTTP methods to exclude from logging (comma-separated)
        /// </summary>
        public string ExcludedHttpMethods { get; set; } = "";

        /// <summary>
        /// Whether to include request headers in logs
        /// </summary>
        public bool IncludeRequestHeaders { get; set; } = false;

        /// <summary>
        /// Whether to include response headers in logs
        /// </summary>
        public bool IncludeResponseHeaders { get; set; } = false;

        /// <summary>
        /// Sensitive headers to mask in logs (comma-separated)
        /// </summary>
        public string SensitiveHeaders { get; set; } = "Authorization,Api-Key,X-API-Key";

        /// <summary>
        /// Sensitive query parameters to mask in logs (comma-separated)
        /// </summary>
        public string SensitiveQueryParameters { get; set; } = "password,token,apikey,secret";

        /// <summary>
        /// Whether to log request/response body
        /// </summary>
        public bool LogBody { get; set; } = true;

        /// <summary>
        /// Content types to exclude from body logging (comma-separated)
        /// </summary>
        public string ExcludedContentTypes { get; set; } = "application/octet-stream,image/,video/,audio/";

        /// <summary>
        /// Application Insights connection string (optional, can be set via configuration)
        /// </summary>
        public string ApplicationInsightsConnectionString { get; set; }

        /// <summary>
        /// Log destination: "Local", "ApplicationInsights", or "Both". Default: "Both"
        /// </summary>
        public string LogDestination { get; set; } = "Both";

        /// <summary>
        /// Whether to write logs locally (using ILogger). Default: true
        /// </summary>
        public bool WriteToLocalLogs { get; set; } = true;

        /// <summary>
        /// Whether to send telemetry to Application Insights. Default: true
        /// </summary>
        public bool WriteToApplicationInsights { get; set; } = true;

        /// <summary>
        /// File logging options (for local file logging)
        /// </summary>
        public FileLoggerOptions FileLogging { get; set; } = new FileLoggerOptions();
    }
}
