# Audit and Error Logging Enhancements

## Overview
The telemetry middleware has been enhanced to capture essential audit information and detailed error diagnostics for compliance, troubleshooting, and security auditing purposes. The audit logging is optimized to capture only essential information to avoid duplication and reduce log size while maintaining compliance requirements.

## New Features

### 1. **Essential User Identity Capture (Audit Trail)**
Captures essential user authentication and authorization information for audit purposes:

- **UserName**: Username or identity name
- **UserId**: User identifier from claims (supports multiple claim types: `NameIdentifier`, `sub`, `user_id`)
- **TenantId**: Tenant identifier (supports `tenant_id`, `TenantId` claims)

**Note**: Only essential user identity fields are captured to avoid duplication and reduce log size. All user claims are not captured in bulk to keep audit logs focused on essential information and prevent duplicate entries.

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

### Essential Audit Information Captured

The audit logging captures only essential information to avoid duplication and reduce log size:

1. **Request Information**: Method, Path, QueryString, RequestBody (payload)
2. **Response Information**: StatusCode
3. **Timing Information**: RequestTimestamp, ResponseTimestamp, TotalDuration
4. **Client Information**: ClientIP, CorrelationId
5. **User Identity**: UserName, UserId, TenantId (essential for audit trail)
6. **Routing Information**: Controller, Action
7. **Error Information**: ExceptionType, ExceptionMessage (when errors occur)

### Example Log Entry (File/Application Insights)

```
Method: POST | Path: /api/users | QueryString: ?page=1 | RequestBody: {"name":"John","email":"john@example.com"} | StatusCode: 200 | RequestTimestamp: 2024-01-15 10:30:45.123 | ResponseTimestamp: 2024-01-15 10:30:45.168 | TotalDuration: 45.234ms | ClientIP: 192.168.1.1 | CorrelationId: abc123-def456 | UserName: john.doe | UserId: 12345 | TenantId: 1 | Controller: UsersController | Action: CreateUser
```

### Example Error Log Entry

```
Method: POST | Path: /api/users | QueryString: ?page=1 | RequestBody: {"name":"John"} | StatusCode: 500 | RequestTimestamp: 2024-01-15 10:30:45.123 | ResponseTimestamp: 2024-01-15 10:30:45.168 | TotalDuration: 23.456ms | ClientIP: 192.168.1.1 | CorrelationId: abc123-def456 | UserName: john.doe | UserId: 12345 | TenantId: 1 | Controller: UsersController | Action: CreateUser | ExceptionType: System.InvalidOperationException | ExceptionMessage: Cannot insert duplicate key | ExceptionLineNumber: 145 | ExceptionFileName: UserRepository.cs | ErrorReason: Internal Server Error - Unexpected server error
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
- **Sensitive Headers**: Configured via `SensitiveHeaders` option
- **Sensitive Query Parameters**: Configured via `SensitiveQueryParameters` option
- **Sensitive Form Fields**: Uses same configuration as query parameters

**Note**: User claims are not captured in bulk to avoid duplication and reduce log size. Only essential identity fields (UserId, TenantId, UserEmail, UserRoles) are captured individually.

### Privacy Compliance
- User identity information is captured only when `IncludeUserIdentity` is enabled
- All sensitive claims are automatically masked
- Consider your organization's privacy policies when enabling user identity capture

## Use Cases

### 1. **Audit Trail**
Track who did what, when, and from where:
- User identity (UserId, UserName, TenantId) - essential fields only
- Request details (Method, Path, QueryString, RequestBody)
- Timestamp information (RequestTimestamp, ResponseTimestamp, TotalDuration)
- Client IP address and CorrelationId

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
   Note: Only essential user identity fields (UserId, UserName, TenantId) are captured to avoid duplication.

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
   - Ensure all sensitive query parameters are configured
   - Request body and query string are automatically masked based on configuration

5. **Monitor Log Sizes**:
   - Audit logging is optimized to capture only essential information
   - Configure appropriate log rotation and retention policies
   - Removed duplicate claims capture to reduce log size

## Changes Made to Address Duplication Issues

1. **Removed Duplicate Claims Capture**: The middleware previously captured all user claims in addition to individual identity fields (UserId, TenantId, UserEmail, UserRoles), causing duplication. This has been removed.

2. **Simplified Audit Logging**: Audit logs now capture only essential information:
   - Request payload (RequestBody)
   - Query string
   - Date/time and duration (RequestTimestamp, ResponseTimestamp, TotalDuration)
   - Essential user identity (UserId, UserName, TenantId)
   - Controller/Action information
   - Error information (when applicable)

3. **Reduced Log Size**: By removing unnecessary fields and duplicate information, log entries are more concise and focused on audit compliance requirements.