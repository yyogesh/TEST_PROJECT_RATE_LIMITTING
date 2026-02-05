using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SEB.FPE.Telemetry
{
    /// <summary>
    /// Extension methods for registering telemetry middleware
    /// </summary>
    public static class TelemetryMiddlewareExtensions
    {
        /// <summary>
        /// Adds telemetry services to the service collection.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddTelemetry(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure options from appsettings.json
            var telemetrySection = configuration.GetSection("Telemetry");
            services.Configure<TelemetryOptions>(telemetrySection);

            // Configure file logger options
            var fileLoggingSection = telemetrySection.GetSection("FileLogging");
            services.Configure<FileLoggerOptions>(fileLoggingSection);

            // Register file logger if file logging is enabled
            var fileLoggingOptions = new FileLoggerOptions();
            fileLoggingSection.Bind(fileLoggingOptions);
            if (fileLoggingOptions.IsEnabled)
            {
                services.AddSingleton<IFileLogger, FileLogger>();
            }

            // Get options to check if Application Insights connection string is provided
            var options = new TelemetryOptions();
            telemetrySection.Bind(options);

            // If Application Insights connection string is provided in Telemetry section, use it
            // Otherwise, it should be configured via ApplicationInsights section (existing setup)
            if (!string.IsNullOrEmpty(options.ApplicationInsightsConnectionString))
            {
                services.AddApplicationInsightsTelemetry(opt =>
                {
                    opt.ConnectionString = options.ApplicationInsightsConnectionString;
                });
            }
            // If Application Insights is already configured elsewhere, we'll use the existing TelemetryClient
            // The middleware will work with or without TelemetryClient - it's optional

            return services;
        }

        /// <summary>
        /// Adds the telemetry middleware to the application pipeline.
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder for chaining</returns>
        public static IApplicationBuilder UseTelemetry(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TelemetryMiddleware>();
        }
    }
}
