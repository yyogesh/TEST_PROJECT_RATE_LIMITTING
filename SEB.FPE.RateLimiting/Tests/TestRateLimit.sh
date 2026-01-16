#!/bin/bash
# Bash script to test rate limiting middleware
# Usage: ./TestRateLimit.sh https://localhost:44301/api/test 150

URL=${1:-"https://localhost:44301/api/test"}
REQUESTS=${2:-150}
DELAY_MS=100

echo "Testing Rate Limiting Middleware"
echo "URL: $URL"
echo "Number of Requests: $REQUESTS"
echo "Delay between requests: ${DELAY_MS}ms"
echo ""

SUCCESS_COUNT=0
RATE_LIMITED_COUNT=0
ERROR_COUNT=0

for i in $(seq 1 $REQUESTS); do
    RESPONSE=$(curl -s -w "\n%{http_code}\n%{header_json}" -k "$URL" 2>&1)
    HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)
    
    if [ "$HTTP_CODE" -eq 200 ]; then
        SUCCESS_COUNT=$((SUCCESS_COUNT + 1))
        REMAINING=$(echo "$RESPONSE" | grep -i "X-RateLimit-Remaining" | cut -d: -f2 | tr -d ' ')
        LIMIT=$(echo "$RESPONSE" | grep -i "X-RateLimit-Limit" | cut -d: -f2 | tr -d ' ')
        echo "Request $i: Success - Remaining: $REMAINING / $LIMIT"
    elif [ "$HTTP_CODE" -eq 429 ]; then
        RATE_LIMITED_COUNT=$((RATE_LIMITED_COUNT + 1))
        echo "Request $i: RATE LIMITED (429)"
    else
        ERROR_COUNT=$((ERROR_COUNT + 1))
        echo "Request $i: Status $HTTP_CODE"
    fi
    
    if [ $i -lt $REQUESTS ]; then
        sleep $(echo "scale=3; $DELAY_MS/1000" | bc)
    fi
done

echo ""
echo "=== Test Summary ==="
echo "Total Requests: $REQUESTS"
echo "Successful: $SUCCESS_COUNT"
echo "Rate Limited: $RATE_LIMITED_COUNT"
echo "Errors: $ERROR_COUNT"
