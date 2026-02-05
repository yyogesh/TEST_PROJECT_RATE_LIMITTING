namespace SEB.FPE.Telemetry
{
    /// <summary>
    /// Configuration options for file logging
    /// </summary>
    public class FileLoggerOptions
    {
        /// <summary>
        /// Whether file logging is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Directory where log files will be written
        /// </summary>
        public string LogDirectory { get; set; } = "Logs";

        /// <summary>
        /// Base name for log files (without extension)
        /// </summary>
        public string LogFileName { get; set; } = "telemetry";

        /// <summary>
        /// Log file extension (including dot)
        /// </summary>
        public string LogFileExtension { get; set; } = ".log";

        /// <summary>
        /// Whether to rotate log files by day (creates new file each day)
        /// </summary>
        public bool RotateByDay { get; set; } = true;

        /// <summary>
        /// Maximum file size in bytes before rotating to a new file. 0 = no size limit
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = 10485760; // 10 MB default

        /// <summary>
        /// Maximum number of log files to keep. 0 = keep all files
        /// </summary>
        public int MaxFilesToKeep { get; set; } = 30; // Keep 30 days/files by default
    }
}
