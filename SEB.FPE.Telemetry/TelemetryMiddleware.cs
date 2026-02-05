using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SEB.FPE.Telemetry
{
    /// <summary>
    /// Middleware for capturing telemetry data (requests, responses, exceptions) and sending to Application Insights
    /// </summary>
    public class TelemetryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TelemetryOptions _options;
        private readonly ILogger<TelemetryMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileLogger _fileLogger;

        public TelemetryMiddleware(
            RequestDelegate next,
            IOptions<TelemetryOptions> options,
            ILogger<TelemetryMiddleware> logger,
            IServiceProvider serviceProvider,
            IFileLogger fileLogger = null)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _fileLogger = fileLogger;
        }

        private TelemetryClient GetTelemetryClient()
        {
            try
            {
                return _serviceProvider?.GetService<TelemetryClient>();
            }
            catch
            {
                return null;
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if telemetry is enabled
            if (!_options.IsEnabled)
            {
                await _next(context);
                return;
            }

            // Check if path is excluded
            if (IsExcludedPath(context.Request.Path.Value))
            {
                await _next(context);
                return;
            }

            // Check if HTTP method is excluded
            if (IsExcludedHttpMethod(context.Request.Method))
            {
                await _next(context);
                return;
            }

            var startTime = DateTime.UtcNow;
            var requestUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            var requestTelemetry = new RequestTelemetry
            {
                Name = $"{context.Request.Method} {context.Request.Path}",
                Url = new Uri(requestUrl),
                Timestamp = startTime
            };

            // Capture request information
            if (_options.LogRequests)
            {
                await CaptureRequestAsync(context, requestTelemetry);
            }

            // Enable response body capture
            var originalBodyStream = context.Response.Body;
            string responseBody = null;

            if (_options.LogResponses)
            {
                using (var responseBodyStream = new MemoryStream())
                {
                    context.Response.Body = responseBodyStream;

                    Exception exception = null;
                    try
                    {
                        await _next(context);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        throw;
                    }
                    finally
                    {
                        // Capture response information
                        if (_options.LogResponses)
                        {
                            responseBody = await CaptureResponseAsync(context, responseBodyStream, originalBodyStream, requestTelemetry, startTime);
                        }

                        // Capture exception if occurred
                        if (exception != null && _options.LogExceptions)
                        {
                            CaptureException(exception, context, requestTelemetry);
                        }

                        // Set response telemetry properties
                        requestTelemetry.Duration = DateTime.UtcNow - startTime;
                        requestTelemetry.ResponseCode = context.Response.StatusCode.ToString();
                        requestTelemetry.Success = context.Response.StatusCode < 400 && exception == null;

                        // Send to Application Insights if enabled
                        if (ShouldWriteToApplicationInsights())
                        {
                            var telemetryClient = GetTelemetryClient();
                            if (telemetryClient != null)
                            {
                                telemetryClient.TrackRequest(requestTelemetry);
                            }
                        }

                        // Write to local logs if enabled (async)
                        if (ShouldWriteToLocalLogs())
                        {
                            await LogTelemetryAsync(context, requestTelemetry, responseBody, exception);
                        }
                    }
                }
            }
            else
            {
                Exception exception = null;
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    if (_options.LogExceptions)
                    {
                        CaptureException(ex, context, requestTelemetry);
                    }
                    throw;
                }
                finally
                {
                    requestTelemetry.Duration = DateTime.UtcNow - startTime;
                    requestTelemetry.ResponseCode = context.Response.StatusCode.ToString();
                    requestTelemetry.Success = context.Response.StatusCode < 400 && exception == null;

                    // Send to Application Insights if enabled
                    if (ShouldWriteToApplicationInsights())
                    {
                        var telemetryClient = GetTelemetryClient();
                        if (telemetryClient != null)
                        {
                            telemetryClient.TrackRequest(requestTelemetry);
                        }
                    }

                    // Write to local logs if enabled (async)
                    if (ShouldWriteToLocalLogs())
                    {
                        await LogTelemetryAsync(context, requestTelemetry, null, exception);
                    }
                }
            }
        }

        private async Task CaptureRequestAsync(HttpContext context, RequestTelemetry requestTelemetry)
        {
            // Set basic request properties
            // Note: HttpMethod is obsolete, using Name property instead which includes method
            requestTelemetry.Properties["HttpMethod"] = context.Request.Method;
            requestTelemetry.Properties["RequestPath"] = context.Request.Path.Value;
            requestTelemetry.Properties["RequestQueryString"] = MaskSensitiveQueryParams(context.Request.QueryString.ToString());
            requestTelemetry.Properties["ClientIpAddress"] = GetClientIpAddress(context);
            requestTelemetry.Properties["UserAgent"] = context.Request.Headers["User-Agent"].ToString();
            requestTelemetry.Properties["RequestScheme"] = context.Request.Scheme;
            requestTelemetry.Properties["RequestHost"] = context.Request.Host.ToString();
            requestTelemetry.Properties["RequestProtocol"] = context.Request.Protocol;
            requestTelemetry.Properties["RequestContentType"] = context.Request.ContentType ?? "";
            requestTelemetry.Properties["RequestContentLength"] = context.Request.ContentLength?.ToString() ?? "0";
            requestTelemetry.Properties["RequestTimestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // Capture all query parameters
            var queryParams = new Dictionary<string, string>();
            foreach (var param in context.Request.Query)
            {
                var key = param.Key;
                var value = MaskSensitiveQueryParam(key, string.Join(",", param.Value));
                queryParams[key] = value;
            }
            if (queryParams.Count > 0)
            {
                requestTelemetry.Properties["RequestQueryParameters"] = JsonSerializer.Serialize(queryParams);
            }

            // Capture route parameters
            var routeData = context.GetRouteData();
            if (routeData?.Values != null && routeData.Values.Count > 0)
            {
                var routeParams = new Dictionary<string, string>();
                foreach (var routeValue in routeData.Values)
                {
                    routeParams[routeValue.Key] = routeValue.Value?.ToString() ?? "";
                }
                requestTelemetry.Properties["RequestRouteParameters"] = JsonSerializer.Serialize(routeParams);
            }

            // Capture request headers if enabled
            if (_options.IncludeRequestHeaders)
            {
                var headers = CaptureHeaders(context.Request.Headers, _options.SensitiveHeaders);
                requestTelemetry.Properties["RequestHeaders"] = JsonSerializer.Serialize(headers);
            }
            else
            {
                // Even if not enabled, capture important headers
                var importantHeaders = new Dictionary<string, string>();
                var headerNames = new[] { "Content-Type", "Accept", "Accept-Language", "Referer", "Origin" };
                foreach (var headerName in headerNames)
                {
                    if (context.Request.Headers.ContainsKey(headerName))
                    {
                        importantHeaders[headerName] = context.Request.Headers[headerName].ToString();
                    }
                }
                if (importantHeaders.Count > 0)
                {
                    requestTelemetry.Properties["RequestImportantHeaders"] = JsonSerializer.Serialize(importantHeaders);
                }
            }

            // Capture request body if enabled
            if (_options.LogBody && context.Request.ContentLength > 0 && context.Request.ContentLength <= _options.MaxRequestBodySize)
            {
                var contentType = context.Request.ContentType ?? "";
                if (!IsExcludedContentType(contentType))
                {
                    context.Request.EnableBuffering();
                    var body = await ReadRequestBodyAsync(context.Request);
                    if (!string.IsNullOrEmpty(body))
                    {
                        requestTelemetry.Properties["RequestBody"] = body;
                    }
                }
            }

            // Capture form data if present
            if (context.Request.HasFormContentType && context.Request.Form != null)
            {
                var formData = new Dictionary<string, string>();
                foreach (var field in context.Request.Form)
                {
                    var key = field.Key;
                    var value = MaskSensitiveFormField(key, string.Join(",", field.Value));
                    formData[key] = value;
                }
                if (formData.Count > 0)
                {
                    requestTelemetry.Properties["RequestFormData"] = JsonSerializer.Serialize(formData);
                }
            }
        }

        private async Task<string> CaptureResponseAsync(HttpContext context, MemoryStream responseBodyStream, Stream originalBodyStream, RequestTelemetry requestTelemetry, DateTime startTime)
        {
            var responseStartTime = DateTime.UtcNow;
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            responseBodyStream.Seek(0, SeekOrigin.Begin);

            // Copy response body back to original stream
            await responseBodyStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            var responseEndTime = DateTime.UtcNow;
            var totalDuration = (responseEndTime - startTime).TotalMilliseconds;
            var responseDuration = (responseEndTime - responseStartTime).TotalMilliseconds;

            // Capture response timing information
            requestTelemetry.Properties["ResponseTimestamp"] = responseEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            requestTelemetry.Properties["RequestStartTime"] = startTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            requestTelemetry.Properties["ResponseStartTime"] = responseStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            requestTelemetry.Properties["ResponseEndTime"] = responseEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            requestTelemetry.Properties["TotalDurationMs"] = totalDuration.ToString("F3");
            requestTelemetry.Properties["ResponseDurationMs"] = responseDuration.ToString("F3");
            requestTelemetry.Properties["TimeBetweenRequestAndResponseMs"] = (responseStartTime - startTime).TotalMilliseconds.ToString("F3");

            // Capture response properties
            requestTelemetry.Properties["ResponseStatusCode"] = context.Response.StatusCode.ToString();
            requestTelemetry.Properties["ResponseContentType"] = context.Response.ContentType ?? "";
            requestTelemetry.Properties["ResponseContentLength"] = context.Response.ContentLength?.ToString() ?? responseBody.Length.ToString();

            // Capture response headers if enabled
            if (_options.IncludeResponseHeaders)
            {
                var headers = CaptureHeaders(context.Response.Headers, _options.SensitiveHeaders);
                requestTelemetry.Properties["ResponseHeaders"] = JsonSerializer.Serialize(headers);
            }
            else
            {
                // Capture important response headers
                var importantHeaders = new Dictionary<string, string>();
                var headerNames = new[] { "Content-Type", "Content-Length", "Location", "Cache-Control" };
                foreach (var headerName in headerNames)
                {
                    if (context.Response.Headers.ContainsKey(headerName))
                    {
                        importantHeaders[headerName] = context.Response.Headers[headerName].ToString();
                    }
                }
                if (importantHeaders.Count > 0)
                {
                    requestTelemetry.Properties["ResponseImportantHeaders"] = JsonSerializer.Serialize(importantHeaders);
                }
            }

            // Capture response body if enabled
            if (_options.LogBody && !string.IsNullOrEmpty(responseBody) && responseBody.Length <= _options.MaxResponseBodySize)
            {
                var contentType = context.Response.ContentType ?? "";
                if (!IsExcludedContentType(contentType))
                {
                    return responseBody;
                }
            }

            return null;
        }

        private void CaptureException(Exception exception, HttpContext context, RequestTelemetry requestTelemetry)
        {
            // Send to Application Insights if enabled
            if (ShouldWriteToApplicationInsights())
            {
                var exceptionTelemetry = new ExceptionTelemetry(exception)
                {
                    SeverityLevel = SeverityLevel.Error,
                    Properties =
                    {
                        ["RequestPath"] = context.Request.Path.Value,
                        ["RequestMethod"] = context.Request.Method,
                        ["ClientIpAddress"] = GetClientIpAddress(context)
                    }
                };

                var telemetryClient = GetTelemetryClient();
                if (telemetryClient != null)
                {
                    telemetryClient.TrackException(exceptionTelemetry);
                }
            }

            // Write to local logs if enabled
            if (ShouldWriteToLocalLogs())
            {
                _logger.LogError(exception, 
                    "Exception occurred during request: {Method} {Path} from {IpAddress}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    GetClientIpAddress(context));
            }
        }

        private async Task LogTelemetryAsync(HttpContext context, RequestTelemetry requestTelemetry, string responseBody, Exception exception)
        {
            var logData = new
            {
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                QueryString = MaskSensitiveQueryParams(context.Request.QueryString.ToString()),
                StatusCode = context.Response.StatusCode,
                Duration = requestTelemetry.Duration.TotalMilliseconds,
                ClientIpAddress = GetClientIpAddress(context),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                HasException = exception != null,
                ExceptionMessage = exception?.Message
            };

            // Build detailed log entry for file logging
            var fileLogEntry = BuildFileLogEntry(context, requestTelemetry, responseBody, exception);

            if (exception != null && _options.LogExceptions)
            {
                var logMessage = $"Request completed with exception: {logData.Method} {logData.Path} - Status: {logData.StatusCode} - Duration: {logData.Duration}ms";
                _logger.LogError(
                    "Request completed with exception: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
                    logData.Method,
                    logData.Path,
                    logData.StatusCode,
                    logData.Duration);
                
                // Write to file if enabled (async)
                if (_fileLogger != null && _options.FileLogging?.IsEnabled == true)
                {
                    await _fileLogger.WriteLogAsync($"{logMessage} | Exception: {exception.Message} | StackTrace: {exception.StackTrace}");
                }
            }
            else if (_options.LogRequests && _options.LogResponses)
            {
                var logMessage = $"Request completed: {logData.Method} {logData.Path} - Status: {logData.StatusCode} - Duration: {logData.Duration}ms";
                _logger.LogInformation(
                    "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
                    logData.Method,
                    logData.Path,
                    logData.StatusCode,
                    logData.Duration);
                
                // Write to file if enabled (async)
                if (_fileLogger != null && _options.FileLogging?.IsEnabled == true)
                {
                    await _fileLogger.WriteLogAsync(fileLogEntry);
                }
            }
            else if (_options.LogRequests)
            {
                var logMessage = $"Request received: {logData.Method} {logData.Path}";
                _logger.LogInformation(
                    "Request received: {Method} {Path}",
                    logData.Method,
                    logData.Path);
                
                // Write to file if enabled (async)
                if (_fileLogger != null && _options.FileLogging?.IsEnabled == true)
                {
                    await _fileLogger.WriteLogAsync(fileLogEntry);
                }
            }
            else if (_options.LogResponses)
            {
                var logMessage = $"Response sent: {logData.Method} {logData.Path} - Status: {logData.StatusCode}";
                _logger.LogInformation(
                    "Response sent: {Method} {Path} - Status: {StatusCode}",
                    logData.Method,
                    logData.Path,
                    logData.StatusCode);
                
                // Write to file if enabled (async)
                if (_fileLogger != null && _options.FileLogging?.IsEnabled == true)
                {
                    await _fileLogger.WriteLogAsync(fileLogEntry);
                }
            }
        }

        private string BuildFileLogEntry(HttpContext context, RequestTelemetry requestTelemetry, string responseBody, Exception exception)
        {
            var sb = new StringBuilder();
            
            // Request Information
            sb.Append($"Method: {context.Request.Method} | ");
            sb.Append($"Path: {context.Request.Path.Value} | ");
            sb.Append($"QueryString: {MaskSensitiveQueryParams(context.Request.QueryString.ToString())} | ");
            sb.Append($"Scheme: {context.Request.Scheme} | ");
            sb.Append($"Host: {context.Request.Host} | ");
            sb.Append($"Protocol: {context.Request.Protocol} | ");
            sb.Append($"ContentType: {context.Request.ContentType ?? "N/A"} | ");
            sb.Append($"ContentLength: {context.Request.ContentLength ?? 0} | ");
            
            // Query Parameters
            if (context.Request.Query.Count > 0)
            {
                var queryParams = new Dictionary<string, string>();
                foreach (var param in context.Request.Query)
                {
                    queryParams[param.Key] = MaskSensitiveQueryParam(param.Key, string.Join(",", param.Value));
                }
                sb.Append($"QueryParameters: {JsonSerializer.Serialize(queryParams)} | ");
            }
            
            // Route Parameters
            var routeData = context.GetRouteData();
            if (routeData?.Values != null && routeData.Values.Count > 0)
            {
                var routeParams = new Dictionary<string, string>();
                foreach (var routeValue in routeData.Values)
                {
                    routeParams[routeValue.Key] = routeValue.Value?.ToString() ?? "";
                }
                sb.Append($"RouteParameters: {JsonSerializer.Serialize(routeParams)} | ");
            }
            
            // Form Data
            if (context.Request.HasFormContentType && context.Request.Form != null && context.Request.Form.Count > 0)
            {
                var formData = new Dictionary<string, string>();
                foreach (var field in context.Request.Form)
                {
                    formData[field.Key] = MaskSensitiveFormField(field.Key, string.Join(",", field.Value));
                }
                sb.Append($"FormData: {JsonSerializer.Serialize(formData)} | ");
            }
            
            // Request Headers
            if (_options.IncludeRequestHeaders)
            {
                var headers = CaptureHeaders(context.Request.Headers, _options.SensitiveHeaders);
                sb.Append($"RequestHeaders: {JsonSerializer.Serialize(headers)} | ");
            }
            
            // Request Body
            if (requestTelemetry.Properties.ContainsKey("RequestBody"))
            {
                sb.Append($"RequestBody: {requestTelemetry.Properties["RequestBody"]} | ");
            }
            
            // Response Information
            sb.Append($"StatusCode: {context.Response.StatusCode} | ");
            sb.Append($"ResponseContentType: {context.Response.ContentType ?? "N/A"} | ");
            sb.Append($"ResponseContentLength: {context.Response.ContentLength ?? 0} | ");
            
            // Timing Information
            if (requestTelemetry.Properties.ContainsKey("TotalDurationMs"))
            {
                sb.Append($"TotalDuration: {requestTelemetry.Properties["TotalDurationMs"]}ms | ");
            }
            if (requestTelemetry.Properties.ContainsKey("ResponseDurationMs"))
            {
                sb.Append($"ResponseDuration: {requestTelemetry.Properties["ResponseDurationMs"]}ms | ");
            }
            if (requestTelemetry.Properties.ContainsKey("TimeBetweenRequestAndResponseMs"))
            {
                sb.Append($"TimeBetweenRequestAndResponse: {requestTelemetry.Properties["TimeBetweenRequestAndResponseMs"]}ms | ");
            }
            
            // Response Headers
            if (_options.IncludeResponseHeaders)
            {
                var responseHeaders = CaptureHeaders(context.Response.Headers, _options.SensitiveHeaders);
                sb.Append($"ResponseHeaders: {JsonSerializer.Serialize(responseHeaders)} | ");
            }
            
            // Response Body
            if (!string.IsNullOrEmpty(responseBody) && _options.LogBody)
            {
                sb.Append($"ResponseBody: {responseBody} | ");
            }
            
            // Client Information
            sb.Append($"ClientIP: {GetClientIpAddress(context)} | ");
            sb.Append($"UserAgent: {context.Request.Headers["User-Agent"]}");
            
            // Exception Information
            if (exception != null)
            {
                sb.Append($" | ExceptionType: {exception.GetType().Name} | ");
                sb.Append($"ExceptionMessage: {exception.Message} | ");
                sb.Append($"StackTrace: {exception.StackTrace}");
            }

            return sb.ToString();
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.Body.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                request.Body.Seek(0, SeekOrigin.Begin);
                return body;
            }
        }

        private Dictionary<string, string> CaptureHeaders(IHeaderDictionary headers, string sensitiveHeaders)
        {
            var sensitiveList = sensitiveHeaders?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(h => h.Trim())
                .ToList() ?? new List<string>();

            var capturedHeaders = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                if (sensitiveList.Any(s => header.Key.Equals(s, StringComparison.OrdinalIgnoreCase)))
                {
                    capturedHeaders[header.Key] = "***MASKED***";
                }
                else
                {
                    capturedHeaders[header.Key] = string.Join(", ", header.Value);
                }
            }
            return capturedHeaders;
        }

        private string MaskSensitiveQueryParams(string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
                return queryString;

            var sensitiveParams = _options.SensitiveQueryParameters?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().ToLower())
                .ToList() ?? new List<string>();

            if (sensitiveParams.Count == 0)
                return queryString;

            var parts = queryString.Split('&');
            var maskedParts = parts.Select(part =>
            {
                if (part.Contains('='))
                {
                    var keyValue = part.Split(new[] { '=' }, 2);
                    var key = keyValue[0].ToLower();
                    if (sensitiveParams.Contains(key))
                    {
                        return $"{keyValue[0]}=***MASKED***";
                    }
                }
                return part;
            });

            return string.Join("&", maskedParts);
        }

        private string MaskSensitiveQueryParam(string key, string value)
        {
            var sensitiveParams = _options.SensitiveQueryParameters?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().ToLower())
                .ToList() ?? new List<string>();

            if (sensitiveParams.Contains(key.ToLower()))
            {
                return "***MASKED***";
            }
            return value;
        }

        private string MaskSensitiveFormField(string key, string value)
        {
            var sensitiveParams = _options.SensitiveQueryParameters?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().ToLower())
                .ToList() ?? new List<string>();

            if (sensitiveParams.Contains(key.ToLower()))
            {
                return "***MASKED***";
            }
            return value;
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP (when behind proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp.Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private bool IsExcludedPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            var excludedPaths = _options.ExcludedPaths?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().ToLower())
                .ToList() ?? new List<string>();

            return excludedPaths.Any(excluded => path.ToLower().StartsWith(excluded));
        }

        private bool IsExcludedHttpMethod(string method)
        {
            if (string.IsNullOrEmpty(_options.ExcludedHttpMethods))
                return false;

            var excludedMethods = _options.ExcludedHttpMethods
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(m => m.Trim().ToUpper())
                .ToList();

            return excludedMethods.Contains(method.ToUpper());
        }

        private bool IsExcludedContentType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            var excludedTypes = _options.ExcludedContentTypes?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .ToList() ?? new List<string>();

            return excludedTypes.Any(excluded => contentType.ToLower().Contains(excluded));
        }

        /// <summary>
        /// Determines if logs should be written to local logs based on configuration
        /// </summary>
        private bool ShouldWriteToLocalLogs()
        {
            // If explicit setting is provided, use it
            if (_options.LogDestination?.Equals("Local", StringComparison.OrdinalIgnoreCase) == true)
                return true;
            if (_options.LogDestination?.Equals("ApplicationInsights", StringComparison.OrdinalIgnoreCase) == true)
                return false;
            if (_options.LogDestination?.Equals("Both", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            // Fallback to explicit WriteToLocalLogs setting
            return _options.WriteToLocalLogs;
        }

        /// <summary>
        /// Determines if telemetry should be sent to Application Insights based on configuration
        /// </summary>
        private bool ShouldWriteToApplicationInsights()
        {
            // If explicit setting is provided, use it
            if (_options.LogDestination?.Equals("Local", StringComparison.OrdinalIgnoreCase) == true)
                return false;
            if (_options.LogDestination?.Equals("ApplicationInsights", StringComparison.OrdinalIgnoreCase) == true)
                return true;
            if (_options.LogDestination?.Equals("Both", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            // Fallback to explicit WriteToApplicationInsights setting
            return _options.WriteToApplicationInsights;
        }
    }
}
