#!/bin/bash
# Bash script to test telemetry middleware
# Run this script after starting the test application

BASE_URL="https://localhost:5001"

echo "=== Telemetry Middleware Test Script ==="
echo ""

# Test 1: Simple GET request
echo "Test 1: Simple GET request"
curl -k -s "$BASE_URL/api/test" | jq .
echo ""

# Test 2: POST request with body
echo "Test 2: POST request with body (test body logging)"
curl -k -s -X POST "$BASE_URL/api/test" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test User","email":"test@example.com","age":30,"password":"secret123"}' | jq .
echo ""

# Test 3: Query parameters (test masking)
echo "Test 3: Query parameters with sensitive data (test masking)"
curl -k -s "$BASE_URL/api/test/query?token=abc123&password=secret&id=123" | jq .
echo "Note: Check logs to verify token and password are masked"
echo ""

# Test 4: Headers (test header masking)
echo "Test 4: Request with headers (test header masking)"
curl -k -s "$BASE_URL/api/test/headers" \
  -H "Authorization: Bearer token123" \
  -H "X-Custom-Header: custom-value" | jq .
echo "Note: Check logs to verify Authorization header is masked"
echo ""

# Test 5: Slow request (test duration)
echo "Test 5: Slow request (test duration tracking)"
curl -k -s "$BASE_URL/api/test/slow" | jq .
echo "Note: Check logs for duration (should be ~2000ms)"
echo ""

# Test 6: Different status codes
echo "Test 6: Different status codes"
for code in 200 404 500; do
  echo "Testing status code $code:"
  curl -k -s -w "\nHTTP Status: %{http_code}\n" "$BASE_URL/api/test/status/$code" | jq .
  echo ""
done

# Test 7: Error endpoint (test exception logging)
echo "Test 7: Error endpoint (test exception logging)"
curl -k -s "$BASE_URL/api/test/error" || echo "Expected exception occurred"
echo "Note: Check logs for exception details"
echo ""

echo "=== All Tests Completed ==="
echo "Check the console output for telemetry logs"
