using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SEB.FPE.RateLimiting
{
    public static class RateLimitMiddlewareExtensions
    {
        /// <summary>
        /// Adds rate limiting services to the service collection.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <param name="dbContextType">Optional DbContext type for SQL Server storage. If null, InMemory storage will be used when StorageType is SqlServer.</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddRateLimiting(
            this IServiceCollection services,
            IConfiguration configuration,
            Type dbContextType = null)
        {
            // Configure options from appsettings.json
            var rateLimitSection = configuration.GetSection("RateLimit");
            services.Configure<RateLimitOptions>(rateLimitSection);

            // Get options for storage type decision
            var options = new RateLimitOptions();
            rateLimitSection.Bind(options);

            // Register storage based on configuration
            if (options.StorageType?.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (dbContextType != null)
                {
                    // Register SqlServer storage with factory to get DbContext from DI
                    services.AddScoped<IRateLimitStorage>(sp =>
                    {
                        var dbContext = sp.GetRequiredService(dbContextType) as DbContext;
                        return new SqlServerRateLimitStorage(dbContext);
                    });
                }
                else
                {
                    // Fallback to InMemory if DbContext type not provided
                    services.AddSingleton<IRateLimitStorage, InMemoryRateLimitStorage>();
                }
            }
            else
            {
                services.AddSingleton<IRateLimitStorage, InMemoryRateLimitStorage>();
            }

            return services;
        }

        /// <summary>
        /// Adds rate limiting services with a specific DbContext type.
        /// </summary>
        public static IServiceCollection AddRateLimiting<TDbContext>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TDbContext : DbContext
        {
            return services.AddRateLimiting(configuration, typeof(TDbContext));
        }

        /// <summary>
        /// Adds the rate limiting middleware to the application pipeline.
        /// </summary>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitMiddleware>();
        }
    }
}
