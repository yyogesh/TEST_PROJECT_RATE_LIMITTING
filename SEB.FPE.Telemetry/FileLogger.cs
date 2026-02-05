using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SEB.FPE.Telemetry
{
    /// <summary>
    /// File logger service for writing telemetry logs to text files
    /// </summary>
    public class FileLogger : IFileLogger, IDisposable
    {
        private readonly FileLoggerOptions _options;
        private readonly ILogger<FileLogger> _logger;
        private readonly SemaphoreSlim _semaphore;
        private string _currentLogFile;
        private long _currentFileSize;

        public FileLogger(IOptions<FileLoggerOptions> options, ILogger<FileLogger> logger)
        {
            _options = options.Value;
            _logger = logger;
            _semaphore = new SemaphoreSlim(1, 1);
            _currentLogFile = GetLogFilePath();
            _currentFileSize = GetCurrentFileSize();
        }

        public async Task WriteLogAsync(string logEntry)
        {
            if (!_options.IsEnabled || string.IsNullOrWhiteSpace(logEntry))
                return;

            await _semaphore.WaitAsync();
            try
            {
                var logFilePath = GetLogFilePath();
                
                // Check if we need to rotate the file
                if (ShouldRotateFile(logFilePath))
                {
                    _currentLogFile = logFilePath;
                    _currentFileSize = 0;
                }

                // Append log entry with timestamp
                var logLine = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] {logEntry}{Environment.NewLine}";
                
                await File.AppendAllTextAsync(_currentLogFile, logLine, Encoding.UTF8);
                
                // Update current file size
                _currentFileSize += Encoding.UTF8.GetByteCount(logLine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to log file");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void WriteLog(string logEntry)
        {
            if (!_options.IsEnabled || string.IsNullOrWhiteSpace(logEntry))
                return;

            _semaphore.Wait();
            try
            {
                var logFilePath = GetLogFilePath();
                
                // Check if we need to rotate the file
                if (ShouldRotateFile(logFilePath))
                {
                    _currentLogFile = logFilePath;
                    _currentFileSize = 0;
                }

                // Append log entry with timestamp
                var logLine = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] {logEntry}{Environment.NewLine}";
                
                File.AppendAllText(_currentLogFile, logLine, Encoding.UTF8);
                
                // Update current file size
                _currentFileSize += Encoding.UTF8.GetByteCount(logLine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to log file");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private string GetLogFilePath()
        {
            var logDirectory = _options.LogDirectory;
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var fileName = GetLogFileName();
            return Path.Combine(logDirectory, fileName);
        }

        private string GetLogFileName()
        {
            var baseFileName = _options.LogFileName ?? "telemetry";
            var extension = _options.LogFileExtension ?? ".log";

            if (_options.RotateByDay)
            {
                // Day-wise rotation: telemetry-2024-01-15.log
                var dateSuffix = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var fileName = $"{baseFileName}-{dateSuffix}{extension}";
                
                // Cleanup old files if MaxFilesToKeep is set
                if (_options.MaxFilesToKeep > 0)
                {
                    CleanupOldFiles(baseFileName, extension);
                }
                
                return fileName;
            }
            else if (_options.MaxFileSizeBytes > 0)
            {
                // Size-based rotation: telemetry-001.log, telemetry-002.log, etc.
                var sequence = GetNextSequenceNumber();
                return $"{baseFileName}-{sequence:D3}{extension}";
            }
            else
            {
                // Single file
                return $"{baseFileName}{extension}";
            }
        }

        private void CleanupOldFiles(string baseFileName, string extension)
        {
            try
            {
                var logDirectory = _options.LogDirectory;
                if (!Directory.Exists(logDirectory))
                    return;

                var pattern = $"{baseFileName}-*{extension}";
                var files = Directory.GetFiles(logDirectory, pattern)
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                if (files.Count > _options.MaxFilesToKeep)
                {
                    var filesToDelete = files.Skip(_options.MaxFilesToKeep);
                    foreach (var file in filesToDelete)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete old log file: {FileName}", file.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during log file cleanup");
            }
        }

        private int GetNextSequenceNumber()
        {
            var logDirectory = _options.LogDirectory;
            var baseFileName = _options.LogFileName ?? "telemetry";
            var extension = _options.LogFileExtension ?? ".log";
            var pattern = $"{baseFileName}-*{extension}";

            var files = Directory.GetFiles(logDirectory, pattern);
            if (files.Length == 0)
                return 1;

            var maxSequence = 0;
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var parts = fileName.Split('-');
                if (parts.Length > 1 && int.TryParse(parts[parts.Length - 1], out var sequence))
                {
                    maxSequence = Math.Max(maxSequence, sequence);
                }
            }

            return maxSequence + 1;
        }

        private bool ShouldRotateFile(string logFilePath)
        {
            // If file path changed (day rotation), we need a new file
            if (logFilePath != _currentLogFile)
                return true;

            // If file doesn't exist, no rotation needed
            if (!File.Exists(logFilePath))
                return false;

            // Check size-based rotation
            if (_options.MaxFileSizeBytes > 0)
            {
                var fileInfo = new FileInfo(logFilePath);
                if (fileInfo.Length >= _options.MaxFileSizeBytes)
                {
                    return true;
                }
            }

            return false;
        }

        private long GetCurrentFileSize()
        {
            if (File.Exists(_currentLogFile))
            {
                var fileInfo = new FileInfo(_currentLogFile);
                return fileInfo.Length;
            }
            return 0;
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }
    }
}
