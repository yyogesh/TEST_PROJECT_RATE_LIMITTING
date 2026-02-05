# SEB.FPE.Telemetry

A middleware project for capturing telemetry data (requests, responses, exceptions) and sending to Application Insights with configurable filtering options.

## Features

- ✅ **Request Logging**: Capture HTTP request details (method, path, headers, body)
- ✅ **Response Logging**: Capture HTTP response details (status code, headers, body)
- ✅ **Exception Logging**: Automatically capture and log exceptions
- ✅ **Application Insights Integration**: Sends telemetry data to Azure Application Insights
- ✅ **Configurable Filtering**: Easily configure what to log and what to exclude
- ✅ **Sensitive Data Masking**: Automatically masks sensitive headers and query parameters
- ✅ **Performance Tracking**: Tracks request duration and performance metrics
- ✅ **Flexible Configuration**: All settings configurable via appsettings.json

## Installation

### 1. Add Project Reference

Add a reference to the telemetry project in your web host project:

```xml
<ProjectReference Include="..\SEB.FPE.Telemetry\SEB.FPE.Telemetry.csproj" />
```

### 2. Register Services

In your `Startup.cs` or `Program.cs`, add the telemetry services:

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

### 3. Register Middleware

In your `Configure` method, add the telemetry middleware:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
    // ... other middleware ...
    
    app.UseRouting();
    app.UseCors();
    app.UseAuthentication();
    
    // Telemetry Middleware - should be early in pipeline to capture all requests
    app.UseTelemetry();
    
    // ... rest of middleware ...
}
```

**Recommended Middleware Order:**
- ✅ After: `UseRouting()`, `UseCors()`, `UseAuthentication()`
- ✅ Before: Your business logic middleware
- ✅ Early in pipeline to capture all requests

## Configuration

Add the following configuration to your `appsettings.json`:

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
  },
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=...;IngestionEndpoint=...;"
  },
  "InsightEnable": {
    "IsEnabled": "true"
  }
}
```

### Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `IsEnabled` | bool | true | Enable/disable telemetry middleware |
| `LogRequests` | bool | true | Log incoming requests |
| `LogResponses` | bool | true | Log outgoing responses |
| `LogExceptions` | bool | true | Log exceptions |
| `MaxRequestBodySize` | int | 10240 | Maximum request body size to log (bytes) |
| `MaxResponseBodySize` | int | 10240 | Maximum response body size to log (bytes) |
| `ExcludedPaths` | string | "/health,/swagger,/ui" | Comma-separated paths to exclude |
| `ExcludedHttpMethods` | string | "" | Comma-separated HTTP methods to exclude |
| `IncludeRequestHeaders` | bool | false | Include request headers in logs |
| `IncludeResponseHeaders` | bool | false | Include response headers in logs |
| `SensitiveHeaders` | string | "Authorization,Api-Key,X-API-Key" | Headers to mask in logs |
| `SensitiveQueryParameters` | string | "password,token,apikey,secret" | Query params to mask in logs |
| `LogBody` | bool | true | Log request/response body |
| `ExcludedContentTypes` | string | "application/octet-stream,image/,video/,audio/" | Content types to exclude from body logging |
| `ApplicationInsightsConnectionString` | string | "" | Optional: Override Application Insights connection string |
| `LogDestination` | string | "Both" | Log destination: "Local", "ApplicationInsights", or "Both" |
| `WriteToLocalLogs` | bool | true | Whether to write logs locally (using ILogger) |
| `WriteToApplicationInsights` | bool | true | Whether to send telemetry to Application Insights |

## Usage Examples

### Example 1: Log Only Requests and Exceptions

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

### Example 2: Exclude Specific Paths

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "ExcludedPaths": "/health,/swagger,/ui,/metrics"
  }
}
```

### Example 3: Exclude Specific HTTP Methods

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "ExcludedHttpMethods": "OPTIONS,HEAD"
  }
}
```

### Example 4: Include Headers in Logs

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

### Example 5: Disable Body Logging

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "LogBody": false
  }
}
```

## What Gets Logged

### Request Information
- HTTP Method (GET, POST, etc.)
- Request Path
- Query String (with sensitive parameters masked)
- Client IP Address
- User Agent
- Request Headers (if enabled)
- Request Body (if enabled and within size limit)

### Response Information
- HTTP Status Code
- Response Duration (milliseconds)
- Response Headers (if enabled)
- Response Body (if enabled and within size limit)

### Exception Information
- Exception Type
- Exception Message
- Stack Trace
- Request Context (path, method, IP address)

## Log Destination Configuration

The middleware supports configurable log destinations:

### Log Destination Options

- **"Local"**: Write logs only to local files using ILogger (standard .NET logging)
- **"ApplicationInsights"**: Send telemetry only to Azure Application Insights
- **"Both"**: Write to both local logs and Application Insights (default)

### Configuration Methods

You can configure the log destination in two ways:

**Method 1: Using LogDestination (Recommended)**
```json
{
  "Telemetry": {
    "LogDestination": "Local"  // or "ApplicationInsights" or "Both"
  }
}
```

**Method 2: Using Individual Settings**
```json
{
  "Telemetry": {
    "WriteToLocalLogs": true,
    "WriteToApplicationInsights": false
  }
}
```

**Note:** If `LogDestination` is set, it takes precedence over individual `WriteToLocalLogs` and `WriteToApplicationInsights` settings.

## Application Insights Integration

The middleware sends telemetry data to Application Insights if:
1. `WriteToApplicationInsights` is `true` (or `LogDestination` is "ApplicationInsights" or "Both")
2. Application Insights is configured (via `ApplicationInsights:ConnectionString` or `Telemetry:ApplicationInsightsConnectionString`)
3. `InsightEnable:IsEnabled` is set to `true`

The following telemetry types are sent:
- **RequestTelemetry**: For each HTTP request
- **ExceptionTelemetry**: For exceptions that occur during request processing

## Security Features

### Sensitive Data Masking

The middleware automatically masks sensitive information:
- **Headers**: Headers listed in `SensitiveHeaders` are masked as `***MASKED***`
- **Query Parameters**: Query parameters listed in `SensitiveQueryParameters` are masked

### Example

If you have a request like:
```
GET /api/users?token=abc123&password=secret&id=123
```

The logged query string will be:
```
token=***MASKED***&password=***MASKED***&id=123
```

## Performance Considerations

- Request/response body logging is limited by `MaxRequestBodySize` and `MaxResponseBodySize`
- Large binary content types are excluded by default
- Excluded paths bypass all telemetry processing
- Telemetry operations are asynchronous and non-blocking

## Troubleshooting

### Telemetry not working?

1. **Check service registration**: Ensure `AddTelemetry` is called in `ConfigureServices`
2. **Check middleware registration**: Ensure `UseTelemetry` is called in `Configure`
3. **Check configuration**: Verify `Telemetry` section exists in `appsettings.json`
4. **Check IsEnabled**: Ensure `Telemetry:IsEnabled` is set to `true`

### Application Insights not receiving data?

1. **Check connection string**: Verify `ApplicationInsights:ConnectionString` is correct
2. **Check InsightEnable**: Ensure `InsightEnable:IsEnabled` is set to `true`
3. **Check TelemetryClient**: Verify Application Insights SDK is properly configured

### Too much data being logged?

1. **Exclude paths**: Add frequently accessed paths to `ExcludedPaths`
2. **Disable body logging**: Set `LogBody` to `false`
3. **Reduce body size limits**: Lower `MaxRequestBodySize` and `MaxResponseBodySize`
4. **Exclude HTTP methods**: Add methods to `ExcludedHttpMethods`

## Future Enhancements

The filtering logic is designed to be easily extensible. Future enhancements could include:
- Custom filter providers
- Dynamic configuration updates
- Custom telemetry processors
- Integration with other telemetry providers
- Advanced sampling strategies
