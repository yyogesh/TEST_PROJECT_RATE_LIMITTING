using System.Threading.Tasks;

namespace SEB.FPE.Telemetry
{
    /// <summary>
    /// Interface for file logging service
    /// </summary>
    public interface IFileLogger
    {
        /// <summary>
        /// Writes a log entry to file
        /// </summary>
        Task WriteLogAsync(string logEntry);

        /// <summary>
        /// Writes a log entry synchronously
        /// </summary>
        void WriteLog(string logEntry);
    }
}
