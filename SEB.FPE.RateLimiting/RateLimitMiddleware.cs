using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace SEB.FPE.RateLimiting
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimitOptions _options;
        private readonly IRateLimitStorage _storage;
        private readonly ILogger<RateLimitMiddleware> _logger;

        public RateLimitMiddleware(
            RequestDelegate next,
            IOptions<RateLimitOptions> options,
            IRateLimitStorage storage,
            ILogger<RateLimitMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _storage = storage;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if rate limiting is enabled
            if (!_options.IsEnabled)
            {
                await _next(context);
                return;
            }

            // Check if path is excluded
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
            var excludedPaths = _options.ExcludedPaths?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().ToLower())
                .ToList() ?? new System.Collections.Generic.List<string>();

            if (excludedPaths.Any(excluded => path.StartsWith(excluded)))
            {
                await _next(context);
                return;
            }

            // Get client IP address
            var ipAddress = GetClientIpAddress(context);

            if (string.IsNullOrEmpty(ipAddress))
            {
                await _next(context);
                return;
            }

            try
            {
                // Increment request count and check if allowed
                var isAllowed = await _storage.IncrementRequestCountAsync(
                    ipAddress,
                    _options.WindowSeconds,
                    _options.PermitLimit);

                // Get updated rate limit info after incrementing
                var rateLimitInfo = await _storage.GetRateLimitInfoAsync(ipAddress, _options.WindowSeconds);

                // Calculate remaining requests and reset time
                var remaining = Math.Max(0, _options.PermitLimit - rateLimitInfo.RequestCount);
                var resetTime = rateLimitInfo.WindowStart.AddSeconds(_options.WindowSeconds);

                // Add rate limit headers
                context.Response.Headers.Add("X-RateLimit-Limit", _options.PermitLimit.ToString());
                context.Response.Headers.Add("X-RateLimit-Remaining", remaining.ToString());
                context.Response.Headers.Add("X-RateLimit-Reset", resetTime.ToUniversalTime().ToString("R"));

                if (!isAllowed)
                {
                    _logger.LogWarning(
                        "Rate limit exceeded for IP: {IpAddress}. RequestCount: {RequestCount}, Limit: {Limit}",
                        ipAddress,
                        rateLimitInfo.RequestCount,
                        _options.PermitLimit);

                    context.Response.StatusCode = _options.StatusCode;
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        error = "Rate limit exceeded",
                        message = _options.Message,
                        retryAfter = Math.Max(0, (int)(resetTime - DateTime.UtcNow).TotalSeconds)
                    };

                    var jsonResponse = JsonSerializer.Serialize(response);
                    await context.Response.WriteAsync(jsonResponse);
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in rate limit middleware for IP: {IpAddress}", ipAddress);
                // On error, allow the request to proceed
                await _next(context);
            }
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

            // Fallback to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
