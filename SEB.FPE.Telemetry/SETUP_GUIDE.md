# Telemetry Middleware Setup Guide

This guide will help you set up and configure the telemetry middleware in your ASP.NET Core application.

## Quick Start

### Step 1: Add Project Reference

Add the telemetry project reference to your web host project's `.csproj` file:

```xml
<ProjectReference Include="..\SEB.FPE.Telemetry\SEB.FPE.Telemetry.csproj" />
```

### Step 2: Register Services

In your `Startup.cs` (or `Program.cs`), add the telemetry service registration in the `ConfigureServices` method:

```csharp
using SEB.FPE.Telemetry;

public IServiceProvider ConfigureServices(IServiceCollection services)
{
    // ... other services ...
    
    // Telemetry - Service Registration
    services.AddTelemetry(_appConfiguration);
    
    // ... rest of services ...
}
```

### Step 3: Register Middleware

In your `Configure` method, add the telemetry middleware:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
    // ... other middleware ...
    
    app.UseRouting();
    app.UseCors();
    app.UseAuthentication();
    
    // Telemetry Middleware - captures requests, responses, and exceptions
    app.UseTelemetry();
    
    // ... rest of middleware ...
}
```

**Important:** Place the telemetry middleware early in the pipeline (after routing, CORS, and authentication) to capture all requests.

### Step 4: Add Configuration

Add the `Telemetry` section to your `appsettings.json`:

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "LogRequests": true,
    "LogResponses": true,
    "LogExceptions": true,
    "MaxRequestBodySize": 10240,
    "MaxResponseBodySize": 10240,
    "ExcludedPaths": "/health,/swagger,/ui",
    "ExcludedHttpMethods": "",
    "IncludeRequestHeaders": false,
    "IncludeResponseHeaders": false,
    "SensitiveHeaders": "Authorization,Api-Key,X-API-Key",
    "SensitiveQueryParameters": "password,token,apikey,secret",
    "LogBody": true,
    "ExcludedContentTypes": "application/octet-stream,image/,video/,audio/",
    "ApplicationInsightsConnectionString": ""
  }
}
```

## Configuration Scenarios

### Scenario 1: Log Only Requests and Exceptions

If you want to log only incoming requests and exceptions (not responses):

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "LogRequests": true,
    "LogResponses": false,
    "LogExceptions": true
  }
}
```

### Scenario 2: Exclude Health Check and Swagger Endpoints

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "ExcludedPaths": "/health,/swagger,/ui,/metrics"
  }
}
```

### Scenario 3: Exclude Specific HTTP Methods

To exclude OPTIONS and HEAD requests:

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "ExcludedHttpMethods": "OPTIONS,HEAD"
  }
}
```

### Scenario 4: Include Headers in Logs

To include request and response headers (with sensitive headers masked):

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "IncludeRequestHeaders": true,
    "IncludeResponseHeaders": true,
    "SensitiveHeaders": "Authorization,Api-Key,X-API-Key,Password"
  }
}
```

### Scenario 5: Disable Body Logging

To disable request/response body logging:

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "LogBody": false
  }
}
```

### Scenario 6: Custom Application Insights Connection String

If you want to override the Application Insights connection string in the Telemetry section:

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "ApplicationInsightsConnectionString": "InstrumentationKey=...;IngestionEndpoint=...;"
  }
}
```

**Note:** If `ApplicationInsightsConnectionString` is empty, the middleware will use the connection string from the `ApplicationInsights` section (if Application Insights is already configured).

### Scenario 7: Write Only to Local Logs

If you want to log only to local files (not send to Application Insights):

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "LogDestination": "Local",
    "WriteToLocalLogs": true,
    "WriteToApplicationInsights": false
  }
}
```

### Scenario 8: Write Only to Application Insights

If you want to send telemetry only to Application Insights (not write to local logs):

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "LogDestination": "ApplicationInsights",
    "WriteToLocalLogs": false,
    "WriteToApplicationInsights": true
  }
}
```

### Scenario 9: Write to Both (Default)

To write to both local logs and Application Insights:

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "LogDestination": "Both",
    "WriteToLocalLogs": true,
    "WriteToApplicationInsights": true
  }
}
```

**Note:** `LogDestination` takes precedence over individual `WriteToLocalLogs` and `WriteToApplicationInsights` settings. You can use either approach:
- Use `LogDestination: "Local"`, `"ApplicationInsights"`, or `"Both"` (simpler)
- Or use `WriteToLocalLogs` and `WriteToApplicationInsights` individually (more granular control)

## Middleware Order

The recommended middleware order is:

```csharp
app.UseAbp();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseJwtTokenMiddleware();
app.UseAntiXssMiddleware();
app.UseRateLimiting();
app.UseTelemetry();  // ← Here
app.UseMiddleware<DecyptParametersMiddleware>();
// ... other middleware ...
```

## Verification

After setup, you should see:

1. **Application Insights**: Request telemetry data in your Application Insights dashboard
2. **Logs**: Structured log entries in your application logs
3. **Exceptions**: Exception telemetry for any errors that occur

## Troubleshooting

### Telemetry not working?

1. Check that `Telemetry:IsEnabled` is set to `true`
2. Verify the middleware is registered: `app.UseTelemetry()`
3. Verify services are registered: `services.AddTelemetry(_appConfiguration)`
4. Check that the path is not excluded in `ExcludedPaths`

### Application Insights not receiving data?

1. Verify `ApplicationInsights:ConnectionString` is correct
2. Check that `InsightEnable:IsEnabled` is set to `true`
3. Ensure Application Insights SDK is properly configured

### Too much data being logged?

1. Add frequently accessed paths to `ExcludedPaths`
2. Set `LogBody` to `false` to disable body logging
3. Reduce `MaxRequestBodySize` and `MaxResponseBodySize`
4. Add HTTP methods to `ExcludedHttpMethods`

## Next Steps

- Review the [README.md](README.md) for detailed documentation
- Customize the configuration based on your needs
- Monitor Application Insights for telemetry data
- Adjust filtering options as needed
