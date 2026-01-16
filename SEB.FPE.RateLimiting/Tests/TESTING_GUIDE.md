# Rate Limiting Middleware - Testing Guide

## Quick Test Configuration

For testing, update your `appsettings.json` with lower limits:

```json
"RateLimit": {
  "IsEnabled": true,
  "StorageType": "InMemory",
  "PermitLimit": 10,        // Lower limit for easy testing
  "WindowSeconds": 60,       // 1 minute window
  "StatusCode": 429,
  "Message": "Rate limit exceeded. Please try again later.",
  "ExcludedPaths": "/health,/swagger,/ui"
}
```

## Testing Methods

### Method 1: Using PowerShell Script (Windows)

1. Open PowerShell in the project root
2. Run the test script:
```powershell
.\src\SEB.FPE.RateLimiting\Tests\TestRateLimit.ps1 -Url "https://localhost:44301/api/your-endpoint" -Requests 15
```

### Method 2: Using Browser Developer Tools

1. Open your browser's Developer Tools (F12)
2. Go to the Console tab
3. Run this JavaScript:
```javascript
async function testRateLimit() {
    const url = 'https://localhost:44301/api/your-endpoint';
    let success = 0, rateLimited = 0;
    
    for (let i = 1; i <= 15; i++) {
        try {
            const response = await fetch(url);
            const remaining = response.headers.get('X-RateLimit-Remaining');
            const limit = response.headers.get('X-RateLimit-Limit');
            
            if (response.status === 200) {
                success++;
                console.log(`Request ${i}: Success - Remaining: ${remaining}/${limit}`);
            } else if (response.status === 429) {
                rateLimited++;
                const data = await response.json();
                console.log(`Request ${i}: RATE LIMITED - ${data.message}`);
            }
        } catch (error) {
            console.error(`Request ${i}: Error - ${error.message}`);
        }
        
        await new Promise(resolve => setTimeout(resolve, 100));
    }
    
    console.log(`\nSummary: ${success} successful, ${rateLimited} rate limited`);
}

testRateLimit();
```

### Method 3: Using Postman/Thunder Client

1. Create a new request to your API endpoint
2. Use the Collection Runner or create a simple script:
   - Set iterations to 15
   - Set delay to 100ms
   - Check the response headers for `X-RateLimit-Remaining`
   - After 10 requests, you should see 429 status code

### Method 4: Using curl (Command Line)

```bash
# Make 15 requests quickly
for i in {1..15}; do
  echo "Request $i:"
  curl -k -i https://localhost:44301/api/your-endpoint
  echo ""
  sleep 0.1
done
```

### Method 5: Using REST Client Extension (VS Code)

1. Install the "REST Client" extension in VS Code
2. Open `TestRateLimit.http` file
3. Click "Send Request" multiple times quickly

## What to Look For

### Successful Request (Status 200)
Response headers should include:
- `X-RateLimit-Limit: 10`
- `X-RateLimit-Remaining: 9` (decreases with each request)
- `X-RateLimit-Reset: [RFC 1123 date]`

### Rate Limited Request (Status 429)
Response body:
```json
{
  "error": "Rate limit exceeded",
  "message": "Rate limit exceeded. Please try again later.",
  "retryAfter": 45
}
```

## Testing Different Scenarios

### Test 1: Basic Rate Limiting
1. Set `PermitLimit: 5` and `WindowSeconds: 60`
2. Make 6 requests quickly
3. First 5 should succeed, 6th should be rate limited

### Test 2: Window Reset
1. Make 5 requests (all succeed)
2. Wait 61 seconds
3. Make another request (should succeed - window reset)

### Test 3: Excluded Paths
1. Make 15 requests to `/health` endpoint
2. All should succeed (not rate limited)

### Test 4: Storage Types
1. Test with `StorageType: "InMemory"` - restart app, limits reset
2. Test with `StorageType: "SqlServer"` - limits persist across restarts

### Test 5: IP Address Detection
1. Test from different IPs (if possible)
2. Each IP should have its own rate limit counter

## Debugging

### Check Logs
The middleware logs warnings when rate limits are exceeded:
```
Rate limit exceeded for IP: 127.0.0.1. RequestCount: 11, Limit: 10
```

### Verify Configuration
Check that your `appsettings.json` configuration is being loaded:
- Ensure `RateLimit` section exists
- Check that `IsEnabled` is `true`
- Verify `StorageType` matches your setup

### Test Storage
For SQL Server storage:
1. Check database table `fpe_RateLimitEntry` for entries
2. Verify entries are created with correct IP addresses
3. Check that old entries are cleaned up

For InMemory storage:
- Restart the application to reset limits
- Check memory usage if testing with many IPs

## Common Issues

### Issue: Rate limiting not working
- Check `IsEnabled` is `true` in appsettings.json
- Verify middleware is registered in `Startup.cs`
- Check middleware order (should be after routing)

### Issue: All requests rate limited
- Check `PermitLimit` value (might be too low)
- Verify IP address detection is working
- Check excluded paths configuration

### Issue: SQL Server storage errors
- Ensure database table exists (run `RateLimitTable.sql`)
- Check connection string is correct
- Verify Entity Framework migrations are applied

## Performance Testing

For load testing, use tools like:
- **Apache Bench (ab)**: `ab -n 1000 -c 10 https://localhost:44301/api/test`
- **wrk**: `wrk -t4 -c100 -d30s https://localhost:44301/api/test`
- **JMeter**: Create a test plan with multiple threads
