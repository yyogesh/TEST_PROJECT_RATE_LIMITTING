# Telemetry Middleware Test Application

This is a test application to verify and demonstrate the telemetry middleware functionality, specifically focusing on local logging.

## Features

- ✅ Tests request logging
- ✅ Tests response logging
- ✅ Tests exception logging
- ✅ Tests query parameter masking
- ✅ Tests header masking
- ✅ Tests different HTTP methods
- ✅ Tests different status codes
- ✅ Tests slow requests (duration tracking)

## Running the Test App

### Prerequisites

- .NET 8.0 SDK
- The `SEB.FPE.Telemetry` project must be built

### Steps

1. **Build the solution:**
   ```bash
   dotnet build
   ```

2. **Run the test app:**
   ```bash
   cd src/SEB.FPE.Telemetry.TestApp
   dotnet run
   ```

3. **Open in browser:**
   - Swagger UI: https://localhost:5001/swagger
   - Home page: https://localhost:5001/

## Test Endpoints

### 1. Simple GET Request
```
GET /api/test
```
Tests basic request/response logging.

### 2. POST Request with Body
```
POST /api/test
Content-Type: application/json

{
  "name": "Test User",
  "email": "test@example.com",
  "age": 30,
  "password": "secret123"
}
```
Tests request body logging and sensitive data masking.

### 3. Error Endpoint
```
GET /api/test/error
```
Tests exception logging. This endpoint intentionally throws an exception.

### 4. Slow Request
```
GET /api/test/slow
```
Tests duration tracking. This endpoint simulates a 2-second delay.

### 5. Query Parameters
```
GET /api/test/query?token=abc123&id=456&password=secret
```
Tests query parameter masking. Check logs to verify `token` and `password` are masked.

### 6. Headers
```
GET /api/test/headers
Headers:
  Authorization: Bearer token123
  X-Custom-Header: custom-value
```
Tests header logging and masking. Check logs to verify `Authorization` header is masked.

### 7. Status Codes
```
GET /api/test/status/200
GET /api/test/status/404
GET /api/test/status/500
```
Tests logging of different HTTP status codes.

## Viewing Logs

### Console Output

When running the application, logs will appear in the console output. Look for log entries like:

```
info: SEB.FPE.Telemetry.TelemetryMiddleware[0]
      Request completed: GET /api/test - Status: 200 - Duration: 15.234ms
```

### Log Levels

The test app is configured to log at `Information` level for telemetry. You can adjust this in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "SEB.FPE.Telemetry": "Information"
    }
  }
}
```

## Configuration

The test app is configured to write **only to local logs** by default. You can change this in `appsettings.json`:

### Local Logs Only (Default)
```json
{
  "Telemetry": {
    "LogDestination": "Local",
    "WriteToLocalLogs": true,
    "WriteToApplicationInsights": false
  }
}
```

### Application Insights Only
```json
{
  "Telemetry": {
    "LogDestination": "ApplicationInsights",
    "WriteToLocalLogs": false,
    "WriteToApplicationInsights": true
  }
}
```

### Both Local and Application Insights
```json
{
  "Telemetry": {
    "LogDestination": "Both",
    "WriteToLocalLogs": true,
    "WriteToApplicationInsights": true
  }
}
```

## Testing Scenarios

### Scenario 1: Verify Request Logging

1. Make a GET request to `/api/test`
2. Check console logs for:
   - Request method (GET)
   - Request path (/api/test)
   - Status code (200)
   - Duration

### Scenario 2: Verify Response Body Logging

1. Make a POST request to `/api/test` with a JSON body
2. Check logs for request body content
3. Verify sensitive fields (like `password`) are handled appropriately

### Scenario 3: Verify Exception Logging

1. Make a GET request to `/api/test/error`
2. Check logs for:
   - Exception message
   - Stack trace
   - Request context (path, method, IP)

### Scenario 4: Verify Query Parameter Masking

1. Make a GET request to `/api/test/query?token=abc123&password=secret&id=123`
2. Check logs to verify:
   - `token` parameter is masked as `***MASKED***`
   - `password` parameter is masked as `***MASKED***`
   - `id` parameter is NOT masked (not in sensitive list)

### Scenario 5: Verify Header Masking

1. Make a GET request to `/api/test/headers` with `Authorization: Bearer token123` header
2. Check logs to verify:
   - `Authorization` header is masked as `***MASKED***`
   - Other headers are logged normally

### Scenario 6: Verify Duration Tracking

1. Make a GET request to `/api/test/slow`
2. Check logs for duration (should be approximately 2000ms)

## Sample Test Scripts

### Using curl

```bash
# Simple GET
curl https://localhost:5001/api/test

# POST with body
curl -X POST https://localhost:5001/api/test \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","email":"test@example.com","age":30,"password":"secret"}'

# Error endpoint
curl https://localhost:5001/api/test/error

# Query parameters
curl "https://localhost:5001/api/test/query?token=abc123&password=secret&id=123"

# With headers
curl https://localhost:5001/api/test/headers \
  -H "Authorization: Bearer token123" \
  -H "X-Custom-Header: custom-value"
```

### Using PowerShell

```powershell
# Simple GET
Invoke-RestMethod -Uri "https://localhost:5001/api/test" -Method Get

# POST with body
$body = @{
    name = "Test User"
    email = "test@example.com"
    age = 30
    password = "secret123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/test" -Method Post -Body $body -ContentType "application/json"

# Error endpoint
Invoke-RestMethod -Uri "https://localhost:5001/api/test/error" -Method Get

# Query parameters
Invoke-RestMethod -Uri "https://localhost:5001/api/test/query?token=abc123&password=secret&id=123" -Method Get
```

## Troubleshooting

### Logs not appearing?

1. Check `Telemetry:IsEnabled` is set to `true`
2. Verify log level is set to `Information` or lower
3. Check that the middleware is registered: `app.UseTelemetry()`

### Want to see more detailed logs?

Change the log level in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "SEB.FPE.Telemetry": "Debug"
    }
  }
}
```

### Want to test Application Insights?

1. Add Application Insights connection string to `appsettings.json`
2. Set `LogDestination` to `"ApplicationInsights"` or `"Both"`
3. Run the app and check Application Insights dashboard

## Next Steps

- Test different configuration combinations
- Verify sensitive data masking works correctly
- Test with different content types
- Test excluded paths functionality
- Test with different HTTP methods
