# Rate Limiting Middleware - Registration Guide

This guide shows how to register the rate limiting middleware in different ASP.NET Core hosting models.

## Current Setup (Startup.cs Pattern)

Your project uses the traditional `Startup.cs` pattern. The middleware is already registered:

### In `Startup.cs` - ConfigureServices Method

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    // ... other service registrations ...
    
    // Rate Limiting - Service Registration
    services.AddRateLimiting<FPEDbContext>(_appConfiguration);
    
    // ... rest of services ...
}
```

### In `Startup.cs` - Configure Method

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
    // ... other middleware ...
    
    app.UseAuthentication();
    app.UseJwtTokenMiddleware();
    app.UseAntiXssMiddleware();
    app.UseRateLimiting();  // ← Rate Limiting Middleware
    
    app.UseMiddleware<DecyptParametersMiddleware>();
    
    // ... rest of middleware ...
}
```

## Alternative: Minimal Hosting Model (Program.cs)

If you're using the newer minimal hosting model (`.NET 6+`), register it in `Program.cs`:

### Program.cs Example

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SEB.FPE.RateLimiting;
using SEB.FPE.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
// ... other services ...

// Rate Limiting - Service Registration
builder.Services.AddRateLimiting<FPEDbContext>(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Rate Limiting - Middleware Registration
app.UseRateLimiting();

app.MapControllers();

app.Run();
```

## Registration Options

### Option 1: InMemory Storage (Default)

```csharp
// In ConfigureServices or builder.Services
services.AddRateLimiting(configuration);
// or
builder.Services.AddRateLimiting(builder.Configuration);
```

### Option 2: SQL Server Storage

```csharp
// In ConfigureServices or builder.Services
services.AddRateLimiting<YourDbContext>(configuration);
// or
builder.Services.AddRateLimiting<YourDbContext>(builder.Configuration);
```

## Configuration

Make sure you have the configuration in `appsettings.json`:

```json
{
  "RateLimit": {
    "IsEnabled": true,
    "StorageType": "InMemory",
    "PermitLimit": 100,
    "WindowSeconds": 60,
    "StatusCode": 429,
    "Message": "Rate limit exceeded. Please try again later.",
    "ExcludedPaths": "/health,/swagger,/ui"
  }
}
```

## Middleware Order

The rate limiting middleware should be placed **after**:
- ✅ `UseRouting()`
- ✅ `UseAuthentication()`
- ✅ `UseCors()`

And **before**:
- ✅ Your custom middleware
- ✅ `UseAuthorization()` (if you want to rate limit before authorization)

### Recommended Order:

```csharp
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseJwtTokenMiddleware();
app.UseAntiXssMiddleware();
app.UseRateLimiting();  // ← Here
app.UseAuthorization();
// ... other middleware ...
```

## Verification

After registration, the middleware will:
1. ✅ Check rate limits on every request (unless excluded)
2. ✅ Add rate limit headers to responses
3. ✅ Return 429 status when limit exceeded
4. ✅ Log rate limit violations

## Troubleshooting

### Middleware not working?

1. **Check service registration**: Ensure `AddRateLimiting` is called in `ConfigureServices`
2. **Check middleware registration**: Ensure `UseRateLimiting` is called in `Configure`
3. **Check configuration**: Verify `RateLimit` section exists in `appsettings.json`
4. **Check IsEnabled**: Ensure `"IsEnabled": true` in configuration
5. **Check middleware order**: Ensure it's placed after routing and authentication

### SQL Server storage not working?

1. **Check DbContext**: Ensure your DbContext has `DbSet<RateLimitEntry> RateLimitEntries`
2. **Check table**: Run `Database/RateLimitTable.sql` to create the table
3. **Check StorageType**: Ensure `"StorageType": "SqlServer"` in configuration
4. **Check DbContext registration**: Ensure your DbContext is registered in DI

## Current Status

✅ **Your current setup is correct!**

The middleware is already registered in:
- `Startup.cs` line 291: `services.AddRateLimiting<FPEDbContext>(_appConfiguration);`
- `Startup.cs` line 334: `app.UseRateLimiting();`

The middleware is active and working! 🎉
