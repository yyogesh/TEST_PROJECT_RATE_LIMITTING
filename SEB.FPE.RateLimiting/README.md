# SEB.FPE.RateLimiting

A standalone, reusable rate limiting middleware library for ASP.NET Core applications.

## Features

- ✅ **IP-based rate limiting** - Rate limit requests by client IP address
- ✅ **Dual storage options** - InMemory (default) or SqlServer
- ✅ **Configurable limits** - Customizable permit limits, time windows, and excluded paths
- ✅ **Standard headers** - X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset
- ✅ **Graceful error handling** - Falls back gracefully on errors
- ✅ **Proxy support** - Handles X-Forwarded-For and X-Real-IP headers
- ✅ **Standalone project** - Can be used as a dependency in multiple projects

## Installation

### As a Project Reference

1. Add a project reference to `SEB.FPE.RateLimiting`:

```xml
<ProjectReference Include="..\SEB.FPE.RateLimiting\SEB.FPE.RateLimiting.csproj" />
```

2. Add the using statement:

```csharp
using SEB.FPE.RateLimiting;
```

### As a NuGet Package (Future)

```bash
dotnet add package SEB.FPE.RateLimiting
```

## Quick Start

### 1. Configuration

Add to `appsettings.json`:

```json
"RateLimit": {
  "IsEnabled": true,
  "StorageType": "InMemory",
  "PermitLimit": 100,
  "WindowSeconds": 60,
  "StatusCode": 429,
  "Message": "Rate limit exceeded. Please try again later.",
  "ExcludedPaths": "/health,/swagger,/ui"
}
```

### 2. Registration

#### For InMemory Storage (Default)

In `Startup.cs` or `Program.cs`:

```csharp
// In ConfigureServices / AddServices
services.AddRateLimiting(configuration);

// In Configure / UseServices
app.UseRateLimiting();
```

#### For SQL Server Storage

1. Add the `RateLimitEntry` DbSet to your DbContext:

```csharp
public class YourDbContext : DbContext
{
    public DbSet<RateLimitEntry> RateLimitEntries { get; set; }
    // ... other DbSets
}
```

2. Run the SQL script to create the table:
   - Location: `Database/RateLimitTable.sql`

3. Register with DbContext type:

```csharp
// In ConfigureServices
services.AddRateLimiting<YourDbContext>(configuration);

// In Configure
app.UseRateLimiting();
```

## Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `IsEnabled` | bool | true | Enable/disable rate limiting |
| `StorageType` | string | "InMemory" | "InMemory" or "SqlServer" |
| `PermitLimit` | int | 100 | Maximum requests per window |
| `WindowSeconds` | int | 60 | Time window in seconds |
| `StatusCode` | int | 429 | HTTP status when limit exceeded |
| `Message` | string | "Rate limit exceeded..." | Error message |
| `ExcludedPaths` | string | "/health,/swagger" | Comma-separated paths to exclude |

## Response Headers

The middleware adds these headers to all responses:

- `X-RateLimit-Limit`: Maximum requests allowed
- `X-RateLimit-Remaining`: Remaining requests in current window
- `X-RateLimit-Reset`: When the rate limit window resets (RFC 1123 format)

## Usage in Multiple Projects

Since this is a separate project, you can:

1. **Reference it in multiple projects** - Add project reference to any ASP.NET Core project
2. **Use different configurations** - Each project can have its own `appsettings.json` configuration
3. **Share the library** - Build once, use in multiple applications

### Example: Using in Project A and Project B

**Project A** (`appsettings.json`):
```json
"RateLimit": {
  "PermitLimit": 50,
  "WindowSeconds": 30
}
```

**Project B** (`appsettings.json`):
```json
"RateLimit": {
  "PermitLimit": 200,
  "WindowSeconds": 60
}
```

Both projects can use the same library with different configurations!

## Testing

See `Tests/TESTING_GUIDE.md` for comprehensive testing instructions.

Quick test using PowerShell:
```powershell
.\Tests\TestRateLimit.ps1 -Url "https://localhost:44301/api/your-endpoint" -Requests 15
```

## Dependencies

- `Microsoft.AspNetCore.Http.Abstractions` (2.2.0+)
- `Microsoft.Extensions.*` (8.0.0+)
- `Microsoft.EntityFrameworkCore` (8.0.8+) - Only for SQL Server storage
- `Abp.Domain.Entities` (9.0.0+) - For RateLimitEntry entity

## Project Structure

```
SEB.FPE.RateLimiting/
├── RateLimitOptions.cs              # Configuration options
├── IRateLimitStorage.cs             # Storage interface
├── InMemoryRateLimitStorage.cs     # In-memory storage
├── SqlServerRateLimitStorage.cs    # SQL Server storage
├── RateLimitEntry.cs                # Entity for SQL Server
├── RateLimitMiddleware.cs          # Main middleware
├── RateLimitMiddlewareExtensions.cs # Extension methods
├── Database/
│   └── RateLimitTable.sql           # SQL script
└── Tests/
    ├── TestRateLimit.ps1
    ├── TestRateLimit.sh
    ├── TestRateLimit.http
    └── TESTING_GUIDE.md
```

## License

Part of the SEB.FPE solution.
