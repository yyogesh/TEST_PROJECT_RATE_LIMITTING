# Audit and Error Logging Enhancements

## Overview
The telemetry middleware has been enhanced to capture comprehensive audit information and detailed error diagnostics for compliance, troubleshooting, and security auditing purposes.

## New Features

### 1. **User Identity and Claims Capture (Audit Trail)**
Captures user authentication and authorization information for audit purposes:

- **IsAuthenticated**: Whether the user is authenticated
- **AuthenticationType**: Type of authentication used (e.g., "Bearer", "Cookie")
- **UserName**: Username or identity name
- **UserId**: User identifier from claims (supports multiple claim types: `NameIdentifier`, `sub`, `user_id`)
- **TenantId**: Tenant identifier (supports `tenant_id`, `TenantId` claims)
- **UserEmail**: User email address
- **UserRoles**: List of user roles
- **UserClaims**: All user claims (when `IncludeRequestHeaders` is enabled, with sensitive claims masked)

**Configuration:**
```json
{
  "Telemetry": {
    "IncludeUserIdentity": true  // Enable/disable user identity capture
  }
}
```

### 2. **Correlation and Request Tracking**
- **CorrelationId**: Unique identifier for tracking requests across services
  - Uses `X-Correlation-ID` header if provided
  - Falls back to `TraceIdentifier` or generates a new GUID
  - Automatically added to response headers
- **TraceId**: ASP.NET Core trace identifier

### 3. **Controller/Action Information**
Captures routing information for better request tracking:
- **Controller**: Controller name handling the request
- **Action**: Action method name
- **Area**: MVC area (if applicable)

### 4. **Enhanced Exception Information**
Comprehensive exception details for debugging and troubleshooting:

#### Basic Exception Info (Always Captured):
- **ExceptionType**: Full type name of the exception
- **ExceptionMessage**: Exception message
- **ExceptionSource**: Source of the exception
- **ExceptionHResult**: Windows error code
- **ExceptionTargetSite**: Method where exception occurred

#### Detailed Exception Info (When `IncludeDetailedExceptionInfo` is enabled):
- **ExceptionLineNumber**: Line number where exception occurred
- **ExceptionFileName**: Source file name
- **ExceptionMethod**: Method name where exception occurred
- **StackTrace**: Full stack trace
- **InnerExceptionType**: Type of inner exception (if any)
- **InnerExceptionMessage**: Inner exception message
- **InnerExceptionLineNumber**: Line number of inner exception
- **InnerExceptionStackTrace**: Stack trace of inner exception
- **AggregateInnerExceptions**: Details of all inner exceptions (for `AggregateException`)
- **DurationAtException**: Request duration when exception occurred

**Configuration:**
```json
{
  "Telemetry": {
    "IncludeDetailedExceptionInfo": true  // Enable detailed exception capture
  }
}
```

### 5. **Error Reason and Category**
For HTTP error responses (status codes >= 400):
- **ErrorReason**: Human-readable description of the error
  - Examples:
    - `400`: "Bad Request - Invalid request parameters or malformed request"
    - `401`: "Unauthorized - Authentication required or failed"
    - `403`: "Forbidden - Access denied, insufficient permissions"
    - `404`: "Not Found - Resource or endpoint not found"
    - `500`: "Internal Server Error - Unexpected server error"
    - And more...
- **ErrorCategory**: Classification of error
  - `ClientError`: 4xx status codes
  - `ServerError`: 5xx status codes
  - `Unknown`: Other status codes

### 6. **Timing Information**
Already captured timing metrics:
- **TotalDurationMs**: Total request duration
- **ResponseDurationMs**: Time to generate response
- **TimeBetweenRequestAndResponseMs**: Processing time before response generation
- **RequestStartTime**: UTC timestamp when request started
- **ResponseStartTime**: UTC timestamp when response generation started
- **ResponseEndTime**: UTC timestamp when response completed

## Configuration Options

### New Configuration Properties

```json
{
  "Telemetry": {
    "IncludeUserIdentity": true,              // Capture user identity for audit
    "IncludeDetailedExceptionInfo": true,    // Capture detailed exception info
    "IncludeCorrelationId": true             // Capture correlation ID (always enabled)
  }
}
```

## Log Entry Format

### Example Log Entry (File/Application Insights)

```
Method: POST | Path: /api/users | QueryString: ?page=1 | Scheme: https | Host: api.example.com | Protocol: HTTP/1.1 | ContentType: application/json | ContentLength: 256 | QueryParameters: {"page":"1"} | RouteParameters: {} | FormData: {} | RequestHeaders: {...} | RequestBody: {...} | StatusCode: 200 | ResponseContentType: application/json | ResponseContentLength: 512 | TotalDuration: 45.234ms | ResponseDuration: 12.456ms | TimeBetweenRequestAndResponse: 32.778ms | ResponseHeaders: {...} | ResponseBody: {...} | ClientIP: 192.168.1.1 | UserAgent: Mozilla/5.0... | CorrelationId: abc123-def456 | TraceId: 00-abc123... | IsAuthenticated: True | UserName: john.doe | UserId: 12345 | TenantId: 1 | UserEmail: john.doe@example.com | UserRoles: ["Admin","User"] | Controller: UsersController | Action: CreateUser
```

### Example Error Log Entry

```
Method: POST | Path: /api/users | ... | StatusCode: 500 | ErrorReason: Internal Server Error - Unexpected server error | ErrorCategory: ServerError | ExceptionType: System.InvalidOperationException | ExceptionMessage: Cannot insert duplicate key | ExceptionSource: MyApp.Data | ExceptionHResult: -2146233079 | ExceptionTargetSite: MyApp.Data.UserRepository.Insert | ExceptionLineNumber: 145 | ExceptionFileName: UserRepository.cs | StackTrace: at MyApp.Data.UserRepository.Insert... | InnerExceptionType: System.Data.SqlClient.SqlException | InnerExceptionMessage: Violation of PRIMARY KEY constraint | InnerExceptionLineNumber: N/A | DurationAtException: 23.456ms
```

## Application Insights Integration

All audit and error information is automatically sent to Application Insights when enabled:

- **RequestTelemetry**: Contains all request/response details and user identity
- **ExceptionTelemetry**: Contains detailed exception information with line numbers
- **Custom Properties**: All captured data is available as custom properties for querying

### Querying in Application Insights

```kusto
// Find all requests by a specific user
requests
| where customDimensions.UserId == "12345"
| project timestamp, name, url, customDimensions

// Find all errors with line numbers
exceptions
| where customDimensions.ExceptionLineNumber != "N/A"
| project timestamp, type, message, customDimensions.ExceptionLineNumber, customDimensions.ExceptionFileName

// Find all requests with errors in a specific controller
requests
| where customDimensions.Controller == "UsersController"
| where success == false
| project timestamp, name, customDimensions.ErrorReason, customDimensions.ErrorCategory
```

## Security Considerations

### Sensitive Data Masking
The middleware automatically masks sensitive information:
- **Sensitive Claims**: Claims containing "password", "token", "secret", "key", "authorization" are masked
- **Sensitive Headers**: Configured via `SensitiveHeaders` option
- **Sensitive Query Parameters**: Configured via `SensitiveQueryParameters` option
- **Sensitive Form Fields**: Uses same configuration as query parameters

### Privacy Compliance
- User identity information is captured only when `IncludeUserIdentity` is enabled
- All sensitive claims are automatically masked
- Consider your organization's privacy policies when enabling user identity capture

## Use Cases

### 1. **Audit Trail**
Track who did what, when, and from where:
- User identity (UserId, UserName, UserEmail)
- Request details (Method, Path, Parameters)
- Timestamp information
- Client IP address

### 2. **Error Diagnostics**
Quickly identify and fix issues:
- Exception line numbers and file names
- Stack traces
- Inner exception details
- Error reasons and categories

### 3. **Performance Monitoring**
Track request performance:
- Total duration
- Response generation time
- Processing time
- Time between request and response

### 4. **Security Auditing**
Monitor suspicious activities:
- Failed authentication attempts
- Unauthorized access attempts
- Unusual request patterns
- Error patterns

## Migration Notes

### Existing Configurations
Existing configurations continue to work. New features are opt-in via configuration:
- `IncludeUserIdentity`: Defaults to `true`
- `IncludeDetailedExceptionInfo`: Defaults to `true`
- `IncludeCorrelationId`: Always enabled (no configuration needed)

### Breaking Changes
None. All enhancements are backward compatible.

## Best Practices

1. **Enable User Identity Capture** for audit compliance:
   ```json
   {
     "IncludeUserIdentity": true
   }
   ```

2. **Enable Detailed Exception Info** for production debugging:
   ```json
   {
     "IncludeDetailedExceptionInfo": true
   }
   ```

3. **Use Correlation IDs** for distributed tracing:
   - Include `X-Correlation-ID` header in client requests
   - The middleware will automatically use it

4. **Review Sensitive Data Configuration**:
   - Ensure all sensitive headers, query parameters, and claims are configured
   - Regularly review and update the sensitive data lists

5. **Monitor Log Sizes**:
   - Detailed logging can generate large log files
   - Configure appropriate log rotation and retention policies
