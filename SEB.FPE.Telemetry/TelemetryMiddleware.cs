using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
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
            
            // Generate correlation/request ID for tracking
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                ?? context.TraceIdentifier 
                ?? Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;
            
            var requestTelemetry = new RequestTelemetry
            {
                Name = $"{context.Request.Method} {context.Request.Path}",
                Url = new Uri(requestUrl),
                Timestamp = startTime,
                Id = correlationId
            };
            
            // Set correlation ID in response headers
            context.Response.Headers["X-Correlation-ID"] = correlationId;

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
            requestTelemetry.Properties["CorrelationId"] = context.Items["CorrelationId"]?.ToString() ?? "";
            requestTelemetry.Properties["TraceId"] = context.TraceIdentifier;
            
            // Capture user identity and claims for audit
            if (_options.IncludeUserIdentity && context.User?.Identity != null)
            {
                requestTelemetry.Properties["IsAuthenticated"] = context.User.Identity.IsAuthenticated.ToString();
                requestTelemetry.Properties["AuthenticationType"] = context.User.Identity.AuthenticationType ?? "";
                requestTelemetry.Properties["UserName"] = context.User.Identity.Name ?? "";
                
                // Capture user ID from claims
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier) 
                    ?? context.User.FindFirst("sub") 
                    ?? context.User.FindFirst("user_id");
                if (userIdClaim != null)
                {
                    requestTelemetry.Properties["UserId"] = userIdClaim.Value;
                }
                
                // Capture tenant ID if available
                var tenantIdClaim = context.User.FindFirst("tenant_id") 
                    ?? context.User.FindFirst("TenantId");
                if (tenantIdClaim != null)
                {
                    requestTelemetry.Properties["TenantId"] = tenantIdClaim.Value;
                }
                
                // Capture email if available
                var emailClaim = context.User.FindFirst(ClaimTypes.Email) 
                    ?? context.User.FindFirst("email");
                if (emailClaim != null)
                {
                    requestTelemetry.Properties["UserEmail"] = emailClaim.Value;
                }
                
                // Capture roles (essential for audit)
                var roles = context.User.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                    .Select(c => c.Value)
                    .ToList();
                if (roles.Count > 0)
                {
                    requestTelemetry.Properties["UserRoles"] = JsonSerializer.Serialize(roles);
                }
                
                // Note: Removed UserClaims capture to avoid duplication
                // Individual claims (UserId, TenantId, UserEmail, UserRoles) are already captured above
            }
            
            // Capture controller and action information
            var routeDataForController = context.GetRouteData();
            if (routeDataForController != null)
            {
                if (routeDataForController.Values.ContainsKey("controller"))
                {
                    requestTelemetry.Properties["Controller"] = routeDataForController.Values["controller"]?.ToString() ?? "";
                }
                if (routeDataForController.Values.ContainsKey("action"))
                {
                    requestTelemetry.Properties["Action"] = routeDataForController.Values["action"]?.ToString() ?? "";
                }
                if (routeDataForController.Values.ContainsKey("area"))
                {
                    requestTelemetry.Properties["Area"] = routeDataForController.Values["area"]?.ToString() ?? "";
                }
            }

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
            // Capture detailed exception information
            var exceptionDetails = GetDetailedExceptionInfo(exception, requestTelemetry);
            
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
                        ["ClientIpAddress"] = GetClientIpAddress(context),
                        ["CorrelationId"] = context.Items["CorrelationId"]?.ToString() ?? "",
                        ["ExceptionType"] = exception.GetType().FullName,
                        ["ExceptionMessage"] = exception.Message,
                        ["ExceptionSource"] = exception.Source ?? "",
                        ["ExceptionHResult"] = exception.HResult.ToString(),
                        ["InnerExceptionType"] = exception.InnerException?.GetType().FullName ?? "",
                        ["InnerExceptionMessage"] = exception.InnerException?.Message ?? ""
                    }
                };
                
                // Add detailed stack trace information
                if (_options.IncludeDetailedExceptionInfo && !string.IsNullOrEmpty(exception.StackTrace))
                {
                    exceptionTelemetry.Properties["StackTrace"] = exception.StackTrace;
                    exceptionTelemetry.Properties["ExceptionLineNumber"] = GetExceptionLineNumber(exception);
                    exceptionTelemetry.Properties["ExceptionFileName"] = GetExceptionFileName(exception);
                }
                
                // Add all exception details
                foreach (var detail in exceptionDetails)
                {
                    exceptionTelemetry.Properties[detail.Key] = detail.Value;
                }

                var telemetryClient = GetTelemetryClient();
                if (telemetryClient != null)
                {
                    telemetryClient.TrackException(exceptionTelemetry);
                }
            }

            // Write to local logs if enabled
            if (ShouldWriteToLocalLogs())
            {
                var exceptionInfo = _options.IncludeDetailedExceptionInfo 
                    ? $" | Type: {exception.GetType().FullName} | Source: {exception.Source} | HResult: {exception.HResult} | Line: {GetExceptionLineNumber(exception)}"
                    : "";
                    
                _logger.LogError(exception, 
                    "Exception occurred during request: {Method} {Path} from {IpAddress}{ExceptionInfo}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    GetClientIpAddress(context),
                    exceptionInfo);
            }
        }
        
        private Dictionary<string, string> GetDetailedExceptionInfo(Exception exception, RequestTelemetry requestTelemetry)
        {
            var details = new Dictionary<string, string>();
            
            if (exception == null)
                return details;
            
            details["ExceptionType"] = exception.GetType().FullName;
            details["ExceptionMessage"] = exception.Message;
            details["ExceptionSource"] = exception.Source ?? "";
            details["ExceptionHResult"] = exception.HResult.ToString();
            details["ExceptionTargetSite"] = exception.TargetSite?.ToString() ?? "";
            
            if (_options.IncludeDetailedExceptionInfo)
            {
                details["StackTrace"] = exception.StackTrace ?? "";
                details["ExceptionLineNumber"] = GetExceptionLineNumber(exception);
                details["ExceptionFileName"] = GetExceptionFileName(exception);
                details["ExceptionMethod"] = exception.TargetSite?.Name ?? "";
                
                // Capture inner exception details
                if (exception.InnerException != null)
                {
                    details["InnerExceptionType"] = exception.InnerException.GetType().FullName;
                    details["InnerExceptionMessage"] = exception.InnerException.Message;
                    details["InnerExceptionStackTrace"] = exception.InnerException.StackTrace ?? "";
                    details["InnerExceptionLineNumber"] = GetExceptionLineNumber(exception.InnerException);
                }
                
                // Capture aggregate exception details
                if (exception is AggregateException aggEx && aggEx.InnerExceptions != null)
                {
                    var innerExceptions = aggEx.InnerExceptions
                        .Select((ex, idx) => new
                        {
                            Index = idx,
                            Type = ex.GetType().FullName,
                            Message = ex.Message,
                            LineNumber = GetExceptionLineNumber(ex)
                        })
                        .ToList();
                    details["AggregateInnerExceptions"] = JsonSerializer.Serialize(innerExceptions);
                }
            }
            
            // Add timing information
            if (requestTelemetry.Properties.ContainsKey("TotalDurationMs"))
            {
                details["DurationAtException"] = requestTelemetry.Properties["TotalDurationMs"];
            }
            
            return details;
        }
        
        private string GetExceptionLineNumber(Exception exception)
        {
            if (exception == null || string.IsNullOrEmpty(exception.StackTrace))
                return "N/A";
            
            try
            {
                // Parse stack trace to find line number
                var stackTrace = new StackTrace(exception, true);
                var frame = stackTrace.GetFrame(0);
                if (frame != null && frame.GetFileLineNumber() > 0)
                {
                    return frame.GetFileLineNumber().ToString();
                }
                
                // Fallback: try to extract from stack trace string
                var lines = exception.StackTrace.Split('\n');
                if (lines.Length > 0)
                {
                    var firstLine = lines[0];
                    // Look for pattern like ":line 123" or "line 123"
                    var lineMatch = System.Text.RegularExpressions.Regex.Match(firstLine, @"(?:line|:line)\s+(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (lineMatch.Success)
                    {
                        return lineMatch.Groups[1].Value;
                    }
                }
            }
            catch
            {
                // If parsing fails, return N/A
            }
            
            return "N/A";
        }
        
        private string GetExceptionFileName(Exception exception)
        {
            if (exception == null || string.IsNullOrEmpty(exception.StackTrace))
                return "N/A";
            
            try
            {
                var stackTrace = new StackTrace(exception, true);
                var frame = stackTrace.GetFrame(0);
                if (frame != null && !string.IsNullOrEmpty(frame.GetFileName()))
                {
                    return Path.GetFileName(frame.GetFileName());
                }
            }
            catch
            {
                // If parsing fails, return N/A
            }
            
            return "N/A";
        }
        
        private string MaskSensitiveClaim(string claimType, string claimValue)
        {
            var sensitiveClaimTypes = new[] { "password", "token", "secret", "key", "authorization" };
            if (sensitiveClaimTypes.Any(s => claimType.ToLower().Contains(s)))
            {
                return "***MASKED***";
            }
            return claimValue;
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
            
            // Essential Request Information
            sb.Append($"Method: {context.Request.Method} | ");
            sb.Append($"Path: {context.Request.Path.Value} | ");
            sb.Append($"QueryString: {MaskSensitiveQueryParams(context.Request.QueryString.ToString())} | ");
            
            // Request Payload (Body)
            if (requestTelemetry.Properties.ContainsKey("RequestBody"))
            {
                sb.Append($"RequestBody: {requestTelemetry.Properties["RequestBody"]} | ");
            }
            
            // Response Status
            sb.Append($"StatusCode: {context.Response.StatusCode} | ");
            
            // Date/Time and Duration (Essential for audit)
            if (requestTelemetry.Properties.ContainsKey("RequestTimestamp"))
            {
                sb.Append($"RequestTimestamp: {requestTelemetry.Properties["RequestTimestamp"]} | ");
            }
            if (requestTelemetry.Properties.ContainsKey("ResponseTimestamp"))
            {
                sb.Append($"ResponseTimestamp: {requestTelemetry.Properties["ResponseTimestamp"]} | ");
            }
            if (requestTelemetry.Properties.ContainsKey("TotalDurationMs"))
            {
                sb.Append($"TotalDuration: {requestTelemetry.Properties["TotalDurationMs"]}ms | ");
            }
            
            // Client Information (Essential for audit)
            sb.Append($"ClientIP: {GetClientIpAddress(context)} | ");
            sb.Append($"CorrelationId: {context.Items["CorrelationId"]}");
            
            // User Identity Information (Essential for audit)
            if (_options.IncludeUserIdentity && context.User?.Identity != null)
            {
                sb.Append($" | UserName: {context.User.Identity.Name ?? "Anonymous"}");
                
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier) 
                    ?? context.User.FindFirst("sub") 
                    ?? context.User.FindFirst("user_id");
                if (userIdClaim != null)
                {
                    sb.Append($" | UserId: {userIdClaim.Value}");
                }
                
                var tenantIdClaim = context.User.FindFirst("tenant_id") 
                    ?? context.User.FindFirst("TenantId");
                if (tenantIdClaim != null)
                {
                    sb.Append($" | TenantId: {tenantIdClaim.Value}");
                }
            }
            
            // Controller/Action Information (Essential for audit)
            var routeDataForLog = context.GetRouteData();
            if (routeDataForLog != null)
            {
                if (routeDataForLog.Values.ContainsKey("controller"))
                {
                    sb.Append($" | Controller: {routeDataForLog.Values["controller"]}");
                }
                if (routeDataForLog.Values.ContainsKey("action"))
                {
                    sb.Append($" | Action: {routeDataForLog.Values["action"]}");
                }
            }
            
            // Exception Information (Essential for error tracking)
            if (exception != null)
            {
                sb.Append($" | ExceptionType: {exception.GetType().FullName} | ");
                sb.Append($"ExceptionMessage: {exception.Message}");
                
                if (_options.IncludeDetailedExceptionInfo)
                {
                    sb.Append($" | ExceptionLineNumber: {GetExceptionLineNumber(exception)} | ");
                    sb.Append($"ExceptionFileName: {GetExceptionFileName(exception)}");
                }
            }
            
            // Error Reason/Status (Essential for error tracking)
            if (context.Response.StatusCode >= 400)
            {
                var errorReason = GetErrorReason(context.Response.StatusCode);
                sb.Append($" | ErrorReason: {errorReason}");
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
        
        private string GetErrorReason(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request - Invalid request parameters or malformed request",
                401 => "Unauthorized - Authentication required or failed",
                403 => "Forbidden - Access denied, insufficient permissions",
                404 => "Not Found - Resource or endpoint not found",
                405 => "Method Not Allowed - HTTP method not supported for this endpoint",
                408 => "Request Timeout - Request took too long to process",
                409 => "Conflict - Resource conflict or duplicate request",
                422 => "Unprocessable Entity - Validation failed",
                429 => "Too Many Requests - Rate limit exceeded",
                500 => "Internal Server Error - Unexpected server error",
                502 => "Bad Gateway - Invalid response from upstream server",
                503 => "Service Unavailable - Service temporarily unavailable",
                504 => "Gateway Timeout - Upstream server timeout",
                _ => $"HTTP {statusCode} - {GetHttpStatusDescription(statusCode)}"
            };
        }
        
        private string GetErrorCategory(int statusCode)
        {
            if (statusCode >= 400 && statusCode < 500)
                return "ClientError";
            if (statusCode >= 500)
                return "ServerError";
            return "Unknown";
        }
        
        private string GetHttpStatusDescription(int statusCode)
        {
            // Common HTTP status descriptions
            var descriptions = new Dictionary<int, string>
            {
                { 400, "Bad Request" },
                { 401, "Unauthorized" },
                { 403, "Forbidden" },
                { 404, "Not Found" },
                { 405, "Method Not Allowed" },
                { 408, "Request Timeout" },
                { 409, "Conflict" },
                { 422, "Unprocessable Entity" },
                { 429, "Too Many Requests" },
                { 500, "Internal Server Error" },
                { 502, "Bad Gateway" },
                { 503, "Service Unavailable" },
                { 504, "Gateway Timeout" }
            };
            
            return descriptions.ContainsKey(statusCode) ? descriptions[statusCode] : "Unknown Status";
        }
    }
}
