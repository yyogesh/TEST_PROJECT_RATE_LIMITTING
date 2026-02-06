# PowerShell script to test telemetry middleware
# Run this script after starting the test application

$baseUrl = "https://localhost:5001"

Write-Host "=== Telemetry Middleware Test Script ===" -ForegroundColor Green
Write-Host ""

# Test 1: Simple GET request
Write-Host "Test 1: Simple GET request" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/test" -Method Get
    Write-Host "✓ Success: $($response.message)" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 2: POST request with body
Write-Host "Test 2: POST request with body (test body logging)" -ForegroundColor Yellow
try {
    $body = @{
        name = "Test User"
        email = "test@example.com"
        age = 30
        password = "secret123"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$baseUrl/api/test" -Method Post -Body $body -ContentType "application/json"
    Write-Host "✓ Success: $($response.message)" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 3: Query parameters (test masking)
Write-Host "Test 3: Query parameters with sensitive data (test masking)" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/test/query?token=abc123&password=secret&id=123" -Method Get
    Write-Host "✓ Success: $($response.message)" -ForegroundColor Green
    Write-Host "  Note: Check logs to verify token and password are masked" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 4: Headers (test header masking)
Write-Host "Test 4: Request with headers (test header masking)" -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer token123"
        "X-Custom-Header" = "custom-value"
    }
    $response = Invoke-RestMethod -Uri "$baseUrl/api/test/headers" -Method Get -Headers $headers
    Write-Host "✓ Success: $($response.message)" -ForegroundColor Green
    Write-Host "  Note: Check logs to verify Authorization header is masked" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 5: Slow request (test duration)
Write-Host "Test 5: Slow request (test duration tracking)" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/test/slow" -Method Get
    Write-Host "✓ Success: $($response.message)" -ForegroundColor Green
    Write-Host "  Note: Check logs for duration (should be ~2000ms)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 6: Different status codes
Write-Host "Test 6: Different status codes" -ForegroundColor Yellow
$statusCodes = @(200, 404, 500)
foreach ($code in $statusCodes) {
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl/api/test/status/$code" -Method Get -UseBasicParsing
        Write-Host "✓ Status $code : Success" -ForegroundColor Green
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq $code) {
            Write-Host "✓ Status $code : Expected error" -ForegroundColor Green
        } else {
            Write-Host "✗ Status $code : Unexpected error" -ForegroundColor Red
        }
    }
}
Write-Host ""

# Test 7: Error endpoint (test exception logging)
Write-Host "Test 7: Error endpoint (test exception logging)" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/test/error" -Method Get
    Write-Host "✗ Unexpected success" -ForegroundColor Red
} catch {
    Write-Host "✓ Expected exception: $($_.Exception.Message)" -ForegroundColor Green
    Write-Host "  Note: Check logs for exception details" -ForegroundColor Cyan
}
Write-Host ""

Write-Host "=== All Tests Completed ===" -ForegroundColor Green
Write-Host "Check the console output for telemetry logs" -ForegroundColor Cyan
