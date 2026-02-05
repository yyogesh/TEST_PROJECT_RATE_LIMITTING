# Rate Limiting Middleware - Setup Instructions

## File to Modify: `Startup.cs`

Location: `src/SEB.FPE.Web.Host/Startup/Startup.cs`

## Step 1: Add Using Statement (if not already present)

At the top of `Startup.cs`, ensure you have:

```csharp
using SEB.FPE.RateLimiting;
```

**Current Status:** ✅ Already added at line 44

## Step 2: Register Services in `ConfigureServices` Method

Find the `ConfigureServices` method (around line 93) and add the rate limiting service registration.

### Option A: InMemory Storage (Simpler - No Database Required)

If you want to use **InMemory** storage (default), use:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    // ... other service registrations ...
    
    services.AddHttpContextAccessor();
    
    // Rate Limiting - InMemory Storage
    services.AddRateLimiting(_appConfiguration);
    
    // ... rest of services ...
}
```

**Current Status:** You have `services.AddRateLimiting<FPEDbContext>(_appConfiguration);` at line 291
- This works for both InMemory and SQL Server
- If you want simpler code for InMemory only, change to: `services.AddRateLimiting(_appConfiguration);`

### Option B: SQL Server Storage (Requires Database)

If you want to use **SQL Server** storage, use:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    // ... other service registrations ...
    
    services.AddHttpContextAccessor();
    
    // Rate Limiting - SQL Server Storage
    services.AddRateLimiting<FPEDbContext>(_appConfiguration);
    
    // ... rest of services ...
}
```

**Current Status:** ✅ Already configured at line 291

## Step 3: Register Middleware in `Configure` Method

Find the `Configure` method (around line 310) and add the middleware to the pipeline.

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
    // ... other middleware ...
    
    app.UseRouting();
    app.UseCors(DefaultCorsPolicyName);
    app.UseAuthentication();
    app.UseJwtTokenMiddleware();
    app.UseAntiXssMiddleware();
    
    // Rate Limiting Middleware
    app.UseRateLimiting();
    
    app.UseMiddleware<DecyptParametersMiddleware>();
    
    // ... rest of middleware ...
}
```

**Current Status:** ✅ Already configured at line 334

## Step 4: Configuration in `appsettings.json`

Ensure your `appsettings.json` has the RateLimit section:

```json
{
  "RateLimit": {
    "IsEnabled": true,
    "StorageType": "InMemory",
    "PermitLimit": 100,
    "WindowSeconds": 60,
    "StatusCode": 429,
    "Message": "Rate limit exceeded. Please try again later.",
    "ExcludedPaths": "/health,/swagger,/ui"
  }
}
```

**Current Status:** ✅ Already configured

## Summary of Current Setup

✅ **Using Statement:** Line 44 - `using SEB.FPE.RateLimiting;`
✅ **Service Registration:** Line 291 - `services.AddRateLimiting<FPEDbContext>(_appConfiguration);`
✅ **Middleware Registration:** Line 334 - `app.UseRateLimiting();`
✅ **Configuration:** `appsettings.json` has RateLimit section

## What You Need to Change

### If Using InMemory Storage Only:

**File:** `src/SEB.FPE.Web.Host/Startup/Startup.cs`

**Line 291:** Change from:
```csharp
services.AddRateLimiting<FPEDbContext>(_appConfiguration);
```

To:
```csharp
services.AddRateLimiting(_appConfiguration);
```

**No other changes needed!** The current setup works for both storage types.

### If Using SQL Server Storage:

1. **Ensure DbContext has RateLimitEntry:**
   - File: `src/SEB.FPE.EntityFrameworkCore/EntityFrameworkCore/FPEDbContext.cs`
   - ✅ Already has: `public virtual DbSet<RateLimitEntry> RateLimitEntries { get; set; }`

2. **Run SQL Script:**
   - File: `src/SEB.FPE.RateLimiting/Database/RateLimitTable.sql`
   - Run this script on your database to create the table

3. **Update appsettings.json:**
   - Change `"StorageType": "InMemory"` to `"StorageType": "SqlServer"`

4. **Keep current registration:**
   - Line 291: `services.AddRateLimiting<FPEDbContext>(_appConfiguration);` ✅

## Verification

After setup, the middleware will:
- ✅ Rate limit requests based on IP address
- ✅ Add rate limit headers to responses
- ✅ Return 429 status when limit exceeded
- ✅ Log rate limit violations

## Testing

See `Tests/TESTING_GUIDE.md` for testing instructions.
