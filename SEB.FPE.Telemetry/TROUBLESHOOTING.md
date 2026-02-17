# Troubleshooting Application Insights Integration

## Issue: No Logs Appearing in Azure Application Insights

### Common Causes and Solutions

### 1. **LogDestination Configuration Conflict**

**Problem:** If `LogDestination` is set to `"Local"`, it will override `WriteToApplicationInsights` and prevent sending to Application Insights.

**Solution:** Change your configuration to one of these:

**Option A: Send Only to Application Insights**
```json
{
  "Telemetry": {
    "LogDestination": "ApplicationInsights",
    "WriteToApplicationInsights": true
  }
}
```

**Option B: Send to Both Local and Application Insights**
```json
{
  "Telemetry": {
    "LogDestination": "Both",
    "WriteToLocalLogs": true,
    "WriteToApplicationInsights": true
  }
}
```

**Option C: Remove LogDestination (Use Individual Settings)**
```json
{
  "Telemetry": {
    "LogDestination": "",
    "WriteToLocalLogs": false,
    "WriteToApplicationInsights": true
  }
}
```

### 2. **Application Insights Not Configured in Startup**

**Check:** Ensure Application Insights is registered in `Startup.cs`:

```csharp
// In ConfigureServices method
if (bool.Parse(_appConfiguration["InsightEnable:IsEnabled"]))
{
    var options = new ApplicationInsightsServiceOptions
    {
        ConnectionString = _appConfiguration["ApplicationInsights:ConnectionString"]
    };
    services.AddApplicationInsightsTelemetry(options);      
}

// Also register telemetry middleware
services.AddTelemetry(_appConfiguration);
```

**Verify:**
- `InsightEnable:IsEnabled` is set to `"true"` (string, not boolean)
- `ApplicationInsights:ConnectionString` is correct
- `services.AddTelemetry(_appConfiguration)` is called

### 3. **Connection String Issues**

**Check your appsettings.json:**
```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=...;IngestionEndpoint=...;"
  },
  "InsightEnable": {
    "IsEnabled": "true"
  }
}
```

**Or use Telemetry section:**
```json
{
  "Telemetry": {
    "ApplicationInsightsConnectionString": "InstrumentationKey=...;IngestionEndpoint=...;"
  }
}
```

**Verify:**
- Connection string is complete and valid
- No typos in the connection string
- Instrumentation key matches your Application Insights resource

### 4. **TelemetryClient Not Resolved**

**Check:** The middleware tries to resolve `TelemetryClient` from DI. If Application Insights is not properly registered, it will be null.

**Solution:** Ensure Application Insights SDK is registered BEFORE the telemetry middleware:

```csharp
// 1. Register Application Insights FIRST
if (bool.Parse(_appConfiguration["InsightEnable:IsEnabled"]))
{
    services.AddApplicationInsightsTelemetry(options);
}

// 2. Then register telemetry middleware
services.AddTelemetry(_appConfiguration);
```

### 5. **Middleware Order**

**Check:** Ensure telemetry middleware is registered in the correct order:

```csharp
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseTelemetry();  // Should be after routing, before your controllers
```

### 6. **Path Exclusions**

**Check:** Your requests might be excluded:

```json
{
  "Telemetry": {
    "ExcludedPaths": "/health,/swagger,/ui"
  }
}
```

If you're testing with `/swagger` or `/health`, those won't be logged.

### 7. **Sampling Enabled**

**Check:** If adaptive sampling is enabled, some requests might be sampled out:

```json
{
  "ApplicationInsights": {
    "EnableAdaptiveSampling": false
  }
}
```

### 8. **Delay in Application Insights**

**Note:** Application Insights can take 1-2 minutes to show data. Wait a few minutes after making requests.

### 9. **Check Application Insights Dashboard**

**Verify:**
1. Go to Azure Portal → Application Insights resource
2. Check "Logs" section
3. Run query: `requests | take 10`
4. Check "Live Metrics" for real-time data

### 10. **Enable Debug Logging**

Add this to see what's happening:

```json
{
  "Logging": {
    "LogLevel": {
      "SEB.FPE.Telemetry": "Debug",
      "Microsoft.ApplicationInsights": "Information"
    }
  }
}
```

### Quick Fix Checklist

1. ✅ Change `LogDestination` from `"Local"` to `"ApplicationInsights"` or `"Both"`
2. ✅ Verify `InsightEnable:IsEnabled` is `"true"` (string)
3. ✅ Verify `ApplicationInsights:ConnectionString` is correct
4. ✅ Ensure `services.AddApplicationInsightsTelemetry()` is called in Startup
5. ✅ Ensure `services.AddTelemetry()` is called in Startup
6. ✅ Ensure `app.UseTelemetry()` is called in Configure
7. ✅ Check that requests are not excluded (not in `ExcludedPaths`)
8. ✅ Wait 1-2 minutes for data to appear in Application Insights
9. ✅ Check Application Insights dashboard in Azure Portal

### Test Configuration

Use this configuration to test Application Insights:

```json
{
  "Telemetry": {
    "IsEnabled": true,
    "LogRequests": true,
    "LogResponses": true,
    "LogExceptions": true,
    "LogDestination": "ApplicationInsights",
    "WriteToApplicationInsights": true,
    "ExcludedPaths": "/swagger"
  },
  "ApplicationInsights": {
    "ConnectionString": "YOUR_CONNECTION_STRING_HERE",
    "EnableAdaptiveSampling": false
  },
  "InsightEnable": {
    "IsEnabled": "true"
  }
}
```

### Debugging Steps

1. **Check if TelemetryClient is resolved:**
   - Add logging in middleware to see if `GetTelemetryClient()` returns null
   - Check application logs for errors

2. **Verify requests are being processed:**
   - Check local logs (if enabled) to see if requests are being captured
   - Add console logging to see if middleware is executing

3. **Test with a simple request:**
   - Make a GET request to a non-excluded endpoint
   - Wait 2-3 minutes
   - Check Application Insights dashboard

4. **Check Application Insights SDK logs:**
   - Enable verbose logging for Application Insights
   - Look for connection errors or authentication issues
