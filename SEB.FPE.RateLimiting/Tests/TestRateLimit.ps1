# PowerShell script to test rate limiting middleware
# Usage: .\TestRateLimit.ps1 -Url "https://localhost:44301/api/test" -Requests 150

param(
    [string]$Url = "https://localhost:44301/api/test",
    [int]$Requests = 150,
    [int]$DelayMs = 100
)

Write-Host "Testing Rate Limiting Middleware" -ForegroundColor Green
Write-Host "URL: $Url" -ForegroundColor Yellow
Write-Host "Number of Requests: $Requests" -ForegroundColor Yellow
Write-Host "Delay between requests: $DelayMs ms" -ForegroundColor Yellow
Write-Host ""

$successCount = 0
$rateLimitedCount = 0
$errorCount = 0

for ($i = 1; $i -le $Requests; $i++) {
    try {
        $response = Invoke-WebRequest -Uri $Url -Method GET -SkipCertificateCheck -ErrorAction SilentlyContinue
        
        $rateLimitLimit = $response.Headers["X-RateLimit-Limit"]
        $rateLimitRemaining = $response.Headers["X-RateLimit-Remaining"]
        $rateLimitReset = $response.Headers["X-RateLimit-Reset"]
        
        if ($response.StatusCode -eq 200) {
            $successCount++
            Write-Host "Request $i`: Success - Remaining: $rateLimitRemaining / $rateLimitLimit" -ForegroundColor Green
        }
        elseif ($response.StatusCode -eq 429) {
            $rateLimitedCount++
            Write-Host "Request $i`: RATE LIMITED (429) - Remaining: $rateLimitRemaining / $rateLimitLimit" -ForegroundColor Red
        }
        else {
            $errorCount++
            Write-Host "Request $i`: Status $($response.StatusCode)" -ForegroundColor Yellow
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 429) {
            $rateLimitedCount++
            Write-Host "Request $i`: RATE LIMITED (429)" -ForegroundColor Red
        }
        else {
            $errorCount++
            Write-Host "Request $i`: Error - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    if ($i -lt $Requests) {
        Start-Sleep -Milliseconds $DelayMs
    }
}

Write-Host ""
Write-Host "=== Test Summary ===" -ForegroundColor Cyan
Write-Host "Total Requests: $Requests" -ForegroundColor White
Write-Host "Successful: $successCount" -ForegroundColor Green
Write-Host "Rate Limited: $rateLimitedCount" -ForegroundColor Red
Write-Host "Errors: $errorCount" -ForegroundColor Yellow
