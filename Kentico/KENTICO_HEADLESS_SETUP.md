# Kentico Xperience Headless CMS Setup - Complete Documentation

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Project Setup](#project-setup)
4. [Admin Configuration](#admin-configuration)
5. [API Testing](#api-testing)
6. [Angular Integration](#angular-integration)
7. [CORS Configuration](#cors-configuration)
8. [Troubleshooting](#troubleshooting)
9. [Key Concepts Summary](#key-concepts-summary)
10. [Quick Reference](#quick-reference)

---

## Overview

### What We Built
A **hybrid CMS setup** using Kentico Xperience where:
- **Content is managed** via the admin interface (traditional CMS)
- **Content is delivered** via GraphQL API (headless)
- **Angular applications** consume the API

### Why Hybrid CMS?
- **Content editors** use the familiar admin UI
- **Developers** use APIs for frontend applications
- **Separation of concerns**: Content management vs. presentation
- **Multi-channel support**: Same content for web, mobile, etc.

---

## Prerequisites

- .NET 8.0 SDK
- SQL Server (local or remote)
- Visual Studio or VS Code
- Kentico Xperience 31.1.2 NuGet packages
- Postman (for API testing)
- Node.js and Angular CLI (for Angular application)

---

## Project Setup

### Step 1: Create ASP.NET Core Web Application

**Why:** Base project for Kentico Xperience.

**Action:**
- Create a new ASP.NET Core Web Application
- Target Framework: .NET 8.0

### Step 2: Install NuGet Packages

**Why:** Required packages for Kentico Xperience functionality.

**Packages to Install:**

```xml
<PackageReference Include="kentico.xperience.admin" Version="31.1.2" />
<PackageReference Include="kentico.xperience.azurestorage" Version="31.1.2" />
<PackageReference Include="kentico.xperience.imageprocessing" Version="31.1.2" />
<PackageReference Include="kentico.xperience.webapp" Version="31.1.2" />
```

**Note:** `kentico.xperience.webapp` includes headless API functionality automatically. No separate `Kentico.Xperience.Headless` package exists or is needed.

### Step 3: Configure Program.cs

**Why:** Enable Kentico features, API controllers, routing, and CORS for Angular integration.

**Complete Program.cs Configuration:**

```csharp
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Headless;

var builder = WebApplication.CreateBuilder(args);

// Enable desired Kentico Xperience features
builder.Services.AddKentico(features =>
{
    // Headless API is included automatically in kentico.xperience.webapp
    // No explicit UseHeadless() call needed
});

// Add CORS support for Angular app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular app URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for Authorization header
    });
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Add both MVC and API controllers
builder.Services.AddControllers(); // Enables API controllers
builder.Services.AddControllersWithViews(); // Enables MVC

var app = builder.Build();

app.InitKentico();

app.UseStaticFiles();
app.UseCookiePolicy();

// CRITICAL: Add routing first
app.UseRouting();

// CRITICAL: Add CORS middleware - Must be after UseRouting, before UseAuthentication
app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.UseKentico();

// Map both API and MVC routes
app.MapControllers(); // Maps API controllers
app.Kentico().MapRoutes(); // Maps Kentico routes (including headless)

app.MapGet("/", () => "The AcmeWeb site has not been configured yet.");

app.Run();
```

**Key Points:**
- `AddControllers()` enables API endpoints
- `MapControllers()` maps API routes
- `Kentico().MapRoutes()` maps headless GraphQL routes
- Headless is included automatically (no `UseHeadless()` needed)
- CORS is configured for Angular app at `http://localhost:4200`
- Middleware order is critical: `UseRouting()` → `UseCors()` → `UseAuthentication()` → `UseAuthorization()`

### Step 4: Configure appsettings.json

**Why:** Database connection and application settings.

**Configuration:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    },
    "XperienceEventLog": {
      "LogLevel": {
        "Default": "Warning",
        "Kentico": "Information",
        "CMS": "Information",
        "Microsoft.AspNetCore.Server.Kestrel": "None"
      }
    }
  },
  "AllowedHosts": "*",
  "CMSHashStringSalt": "3f45af6f-2e78-4302-94d5-80d538a4cf0b",
  "ConnectionStrings": {
    "CMSConnectionString": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=headless;Integrated Security=True;Persist Security Info=False;Connect Timeout=60;Encrypt=False;Current Language=English;"
  }
}
```

**Note:** Replace the connection string with your SQL Server details.

---

## Admin Configuration

### Step 1: Access Admin Interface

**Why:** Manage content, channels, and settings.

**Action:**
- Run the application
- Navigate to: `http://localhost:43907/admin`
- Login with administrator credentials

**Default URL Pattern:** `/admin`

### Step 2: Create Headless Channel

**Why:** Channels organize content and provide API endpoints. Each channel has its own GraphQL endpoint.

**Steps:**
1. Go to **Configuration → Channel management**
2. Click **"New channel"**
3. Select **"Headless channel"**
4. Configure:
   - **Channel name:** "Angular App" (or your choice)
   - **Channel type:** "Headless" (auto-selected)
   - **Channel size:** "Standard channel"
   - **Primary language:** "English" (or your default)
5. Click **"SAVE"**

**Result:**
- Channel created
- GraphQL endpoint generated
- Channel identifier (GUID) created

**Important:** Note the channel identifier from the **"GraphQL API endpoint"** section (e.g., `ca81ab98-d0fa-44b4-b6e8-4b38b34cb8f9`). This is used in your API URL.

### Step 3: Create Content Type

**Why:** Define the structure of your content. Content types define what fields your content will have.

**Steps:**
1. Go to **Configuration → Content types**
2. Click **"NEW CONTENT TYPE"**
3. Configure **General** tab:
   - **Content type name:** "Article"
   - **Code name - Namespace:** "Article"
   - **Code name - Name:** "Article"
   - **Use for:** Select **"Headless"** (required for headless channels)
4. Click **"SAVE"**

**Result:** Content type "Article.Article" created (namespace.name format)

**Note:** The GraphQL field name will be `articleArticle` (camelCase of namespace + name).

### Step 4: Add Fields to Content Type

**Why:** Define the data structure for articles. Fields become GraphQL fields.

**Steps:**
1. Go to **Configuration → Content types → Article → Fields** tab
2. Click **"NEW FIELD"**
3. Add fields:

   **Field 1: Title**
   - **Field name:** "Title"
   - **Data type:** "Text"
   - **Required:** Yes
   - Click **"SAVE"**

   **Field 2: Body**
   - **Field name:** "Body"
   - **Data type:** "Long text" or "Rich text (HTML)"
   - **Required:** Yes
   - Click **"SAVE"**

**Result:** Content type has fields: Title (Text), Body (Long text)

**Note:** Field code names are used in GraphQL queries (usually lowercase, e.g., `title`, `body`). For String fields, query them directly without `{ value }` wrapper.

### Step 5: Assign Content Type to Channel

**Why:** Make the content type available in the headless channel. The GraphQL endpoint is inactive until at least one content type is assigned.

**Steps:**
1. Go to **Channel management → Angular App → Allowed content types** tab
2. Click **"SELECT CONTENT TYPES"**
3. Select **"Article"**
4. Click **"SAVE"**

**Result:**
- Content type assigned to channel
- GraphQL schema updated
- Endpoint becomes **active**

**Important:** The GraphQL endpoint shows as "inactive" until at least one content type is assigned. After assignment, it becomes "active".

### Step 6: Generate API Key

**Why:** Secure the GraphQL API endpoint. All requests must include a valid API key in the Authorization header.

**Steps:**
1. Go to **Channel management → Angular App → API keys** tab
2. Click **"Generate new API key"** or **"Create API key"**
3. Copy and securely store the API key

**Result:** API key generated for authentication

**Usage:** Include in requests as: `Authorization: Bearer your-api-key-here`

**Important:** Keep this key secure. It provides access to your content via the API.

### Step 7: Create Headless Content Items

**Why:** Create actual content to be delivered via API. Only published items are accessible via the API.

**Steps:**
1. Go to **Channel management → Angular App → List of headless items**
2. Click **"NEW HEADLESS ITEM"**
3. Select **"Article"** as content type
4. Fill in fields:
   - **Title:** "My First Article"
   - **Body:** "Content text here"
5. Click **"SAVE"**
6. Click **"PUBLISH"** (required for API access)

**Result:** Published headless content item

**Important:** 
- Only **published** items are available via API
- Draft items are not accessible
- You can create multiple items of the same content type

---

## API Testing

### Step 1: Test Custom API Endpoint

**Why:** Verify API controllers are working before testing GraphQL.

**Create Test Controller:**

```csharp
// Controllers/ContentController.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CMS.ContentEngine;
using CMS.Websites;

namespace Kentico.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContentController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { 
                message = "API is working!",
                timestamp = DateTime.Now 
            });
        }

        [HttpGet("pages")]
        public async Task<IActionResult> GetPages()
        {
            // This is a simple test endpoint
            // You can expand this to query Kentico content
            return Ok(new { 
                pages = new[] { "Page 1", "Page 2" },
                message = "Custom API endpoint working"
            });
        }
    }
}
```

**Test:**
```
GET http://localhost:43907/api/content/test
```

**Expected Response:**
```json
{
  "message": "API is working!",
  "timestamp": "2026-02-15T..."
}
```

### Step 2: Test GraphQL Endpoint

**Why:** Verify headless GraphQL API is working and returning content.

**Endpoint Format:**
```
POST http://localhost:43907/graphql/[channel-identifier]
```

**Example:**
```
POST http://localhost:43907/graphql/ca81ab98-d0fa-44b4-b6e8-4b38b34cb8f9
```

**Postman Configuration:**
- **Method:** POST
- **URL:** `http://localhost:43907/graphql/ca81ab98-d0fa-44b4-b6e8-4b38b34cb8f9`
- **Headers:**
  - `Content-Type: application/json`
  - `Authorization: Bearer your-api-key-here`
- **Body (raw JSON):**
```json
{
  "query": "{ articleArticle { title body } }"
}
```

**Expected Response:**
```json
{
  "data": {
    "articleArticle": {
      "title": "My First Article",
      "body": "<p>Content text here</p>"
    }
  }
}
```

**PowerShell Test Command:**
```powershell
$headers = @{
    "Content-Type" = "application/json"
    "Authorization" = "Bearer your-api-key-here"
}

$body = @{
    query = "{ articleArticle { title body } }"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:43907/graphql/ca81ab98-d0fa-44b4-b6e8-4b38b34cb8f9" `
    -Method Post `
    -Body $body `
    -Headers $headers
```

### Step 3: GraphQL Query Structure

**Why:** Understand how to query content correctly.

**Content Type Naming:**
- Content type: `"Article.Article"` (namespace.name)
- GraphQL field: `articleArticle` (camelCase of namespace + name)

**Basic Query:**
```json
{
  "query": "{ articleArticle { title body } }"
}
```

**Field Types:**
- **String fields:** Query directly (e.g., `title`, `body`)
- **Complex fields:** May require `{ value }` wrapper
- **Collections:** May require `nodes` or `items`

**Common Patterns:**

**Single Item:**
```json
{
  "query": "{ articleArticle { title body } }"
}
```

**Collection (if multiple items):**
```json
{
  "query": "{ articleArticle { nodes { title body } } }"
}
```

**With Filters:**
```json
{
  "query": "{ articleArticle(where: { title: { eq: \"My Title\" } }) { title body } }"
}
```

**Important Notes:**
- String fields like `title` and `body` are queried directly (no `{ value }` wrapper)
- The response structure depends on whether you have one item or multiple items
- If multiple items exist, the response may have a `nodes` property

---

## Angular Integration

### Step 1: Create Angular Application

**Why:** Frontend application to consume Kentico API.

**Action:**
```bash
ng new KenticoAngularApp
cd KenticoAngularApp
```

**Note:** This documentation assumes Angular 21 with standalone components and signals.

### Step 2: Configure Angular App

**Why:** Enable HttpClient and configure providers.

**app.config.ts:**
```typescript
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()) // Required for HttpClient in Angular 21
  ]
};
```

### Step 3: Create Environment Configuration

**Why:** Manage API URLs and keys per environment.

**src/environments/environment.development.ts:**
```typescript
export const environment = {
  production: false,
  kenticoApiUrl: 'http://localhost:43907/graphql/ca81ab98-d0fa-44b4-b6e8-4b38b34cb8f9',
  kenticoApiKey: 'your-api-key-here' // Replace with your actual API key from Kentico admin
};
```

**Important:** Replace `'your-api-key-here'` with the actual API key from Kentico admin.

### Step 4: Create Kentico Service

**Why:** Centralize API calls and handle authentication.

**src/app/services/kentico.service.ts:**
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError, retry } from 'rxjs/operators';
import { environment } from '../../environments/environment.development';

export interface Article {
  id?: string;
  title: string;
  body: string;
}

interface GraphQLResponse {
  data?: {
    articleArticle?: Article | Article[] | { nodes: Article[] };
  };
  errors?: Array<{ message: string }>;
}

@Injectable({
  providedIn: 'root'
})
export class KenticoService {
  private http = inject(HttpClient);
  private apiUrl = environment.kenticoApiUrl;
  private apiKey = environment.kenticoApiKey;

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${this.apiKey}`
    });
  }

  getArticles(): Observable<Article[]> {
    const headers = this.getHeaders();
    
    const query = {
      query: `{
        articleArticle {
          title
          body
        }
      }`
    };

    return this.http.post<GraphQLResponse>(this.apiUrl, query, { headers })
      .pipe(
        retry(2), // Retry failed requests up to 2 times
        map((response): Article[] => {
          if (response.errors) {
            throw new Error(response.errors[0].message);
          }
          
          const articleData = response.data?.articleArticle;
          
          if (!articleData) {
            return [];
          }
          
          // Check if it's an array
          if (Array.isArray(articleData)) {
            return articleData;
          }
          
          // Check if it has nodes property (collection structure)
          if ('nodes' in articleData && Array.isArray(articleData.nodes)) {
            return articleData.nodes;
          }
          
          // Single item - wrap in array
          if ('title' in articleData && 'body' in articleData) {
            return [articleData as Article];
          }
          
          return [];
        }),
        catchError(this.handleError)
      );
  }

  private handleError(error: any): Observable<never> {
    let errorMessage = 'An unknown error occurred';
    
    if (error.error?.errors) {
      errorMessage = error.error.errors[0].message;
    } else if (error.message) {
      errorMessage = error.message;
    }
    
    console.error('Kentico API Error:', error);
    return throwError(() => new Error(errorMessage));
  }
}
```

**Key Points:**
- Uses `inject(HttpClient)` for Angular 21 dependency injection
- Handles different response structures (single item, array, collection with nodes)
- Includes retry logic for robustness
- Proper error handling with TypeScript type narrowing

### Step 5: Create Article Component

**Why:** Display content in your Angular application.

**src/app/components/article/article.ts:**
```typescript
import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { KenticoService, Article } from '../../services/kentico.service';

@Component({
  selector: 'app-article',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './article.html',
  styleUrls: ['./article.scss']
})
export class ArticleComponent implements OnInit {
  // Using Angular 21 signals
  articles = signal<Article[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  // Computed signals for derived state
  hasArticles = computed(() => this.articles().length > 0);
  isLoading = computed(() => this.loading());

  constructor(private kenticoService: KenticoService) { }

  ngOnInit(): void {
    this.loadArticles();
  }

  loadArticles(): void {
    this.loading.set(true);
    this.error.set(null);

    this.kenticoService.getArticles().subscribe({
      next: (data) => {
        this.articles.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(error.message || 'Failed to load articles');
        this.loading.set(false);
        console.error('Error loading articles:', error);
      }
    });
  }

  refreshArticles(): void {
    this.loadArticles();
  }
}
```

**src/app/components/article/article.html:**
```html
<div class="article-container">
  <div class="header">
    <h1>Articles from Kentico CMS</h1>
    <button (click)="refreshArticles()" [disabled]="isLoading()" class="refresh-btn">
      {{ isLoading() ? 'Loading...' : 'Refresh' }}
    </button>
  </div>

  <!-- Loading State -->
  @if (isLoading()) {
    <div class="loading">
      <p>Loading articles...</p>
    </div>
  }

  <!-- Error State -->
  @if (error() && !isLoading()) {
    <div class="error">
      <p><strong>Error:</strong> {{ error() }}</p>
      <button (click)="loadArticles()">Try Again</button>
    </div>
  }

  <!-- Articles List -->
  @if (!isLoading() && !error()) {
    <div class="articles-list">
      @if (!hasArticles()) {
        <div class="no-articles">
          <p>No articles found.</p>
        </div>
      }

      @for (article of articles(); track article.id || article.title) {
        <div class="article-card">
          <h2 class="article-title">{{ article.title }}</h2>
          <div class="article-body" [innerHTML]="article.body"></div>
        </div>
      }
    </div>
  }
</div>
```

**Key Points:**
- Uses Angular 21 signals for state management
- Uses `@if` and `@for` control flow (Angular 21 syntax)
- Handles loading, error, and success states
- Displays articles with title and body

### Step 6: Update Root Component

**src/app/app.ts:**
```typescript
import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ArticleComponent } from './components/article/article';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ArticleComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('Kentico Headless CMS - Angular App');
}
```

**src/app/app.html:**
```html
<div class="app-container">
  <header>
    <h1>{{ title() }}</h1>
  </header>
  <main>
    <app-article></app-article>
  </main>
</div>
```

---

## CORS Configuration

### Why CORS is Needed

**Why:** Browsers enforce the Same-Origin Policy. When your Angular app (running on `http://localhost:4200`) tries to call the Kentico API (running on `http://localhost:43907`), the browser blocks the request unless CORS is properly configured.

### Configuration in Program.cs

**Why:** Allow Angular app to make requests to Kentico API.

**Already included in Step 3 of Project Setup, but here's the key section:**

```csharp
// Add CORS support for Angular app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular app URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for Authorization header
    });
});

// ... in middleware pipeline ...

// CRITICAL: Add routing first
app.UseRouting();

// CRITICAL: Add CORS middleware - Must be after UseRouting, before UseAuthentication
app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();
```

**Critical Middleware Order:**
1. `UseRouting()` - Must be first
2. `UseCors()` - Must be after routing, before authentication
3. `UseAuthentication()` - After CORS
4. `UseAuthorization()` - After authentication

**Why this order matters:**
- CORS must be applied after routing determines the endpoint
- CORS must be applied before authentication checks
- Incorrect order can cause CORS errors even with correct configuration

---

## Troubleshooting

### Issue 1: 404 Not Found on GraphQL Endpoint

**Cause:** Wrong endpoint URL or channel not configured.

**Solutions:**
1. Verify channel identifier from **Channel management → Angular App → General tab**
2. Ensure content type is assigned to channel
3. Check endpoint shows as **"active"** in admin
4. Use correct URL format: `http://localhost:43907/graphql/[channel-identifier]`

### Issue 2: 400 Bad Request - Bearer Token Missing

**Cause:** API key not included in request.

**Solution:**
1. Generate API key from **Channel management → Angular App → API keys**
2. Add header: `Authorization: Bearer your-api-key`
3. Ensure the key is active and not expired
4. In Angular, verify `environment.kenticoApiKey` is set correctly

### Issue 3: Field Does Not Exist Error

**Cause:** Wrong field name or structure.

**Solutions:**
1. Check exact field code names in **Content types → Article → Fields**
2. Use correct GraphQL field name (content type namespace + name in camelCase)
3. For String fields, query directly (no `{ value }` wrapper)
4. Example: `{ articleArticle { title body } }` not `{ articleArticle { title { value } } }`

### Issue 4: Empty Response

**Cause:** No published content items.

**Solution:**
1. Ensure content items are **published** (Status: Published)
2. Create and publish at least one content item
3. Verify the content item is assigned to the correct channel

### Issue 5: Introspection Disabled

**Cause:** Security feature in Kentico.

**Solution:**
- Introspection queries are disabled by default for security
- Use trial and error or check documentation for available fields
- Test queries in Postman first before implementing in Angular

### Issue 6: UseHeadless() Method Not Found

**Cause:** Extension method not recognized.

**Solution:**
- Headless is included automatically in `kentico.xperience.webapp`
- No explicit `UseHeadless()` call needed
- Ensure `using Kentico.Xperience.Headless;` is present (optional, for IntelliSense)

### Issue 7: CORS Error in Browser

**Cause:** CORS not configured or middleware order incorrect.

**Solutions:**
1. Verify CORS policy is added in `Program.cs`
2. Check middleware order: `UseRouting()` → `UseCors()` → `UseAuthentication()`
3. Ensure Angular URL matches `WithOrigins()` value
4. Verify `AllowCredentials()` is included (required for Authorization header)
5. Check browser console for specific CORS error message

### Issue 8: TypeScript Error in Angular Service

**Cause:** Type narrowing issue in response handling.

**Solution:**
- Use explicit type assertions and `in` operator checks
- Ensure all branches of `map` operator return `Article[]`
- See the `kentico.service.ts` example above for correct implementation

### Issue 9: Unauthenticated, Token Invalid or Disabled

**Cause:** API key is placeholder or incorrect.

**Solution:**
1. Check `environment.development.ts` - ensure `kenticoApiKey` is not `'your-api-key-here'`
2. Copy the actual API key from **Channel management → Angular App → API keys**
3. Verify the API key is active in Kentico admin
4. Restart Angular dev server after updating environment file

---

## Key Concepts Summary

### Channel
- **Purpose:** Organizes content and provides API endpoints
- **Requirement:** Must have at least one assigned content type
- **Location:** Configuration → Channel management
- **Result:** Generates unique GraphQL endpoint URL

### Content Type
- **Purpose:** Defines content structure (fields)
- **Requirement:** Must be set to "Headless" for headless channels
- **Result:** Fields become GraphQL fields
- **Location:** Configuration → Content types
- **Naming:** GraphQL field = camelCase(namespace + name), e.g., "Article.Article" → `articleArticle`

### Content Items
- **Purpose:** Actual content instances
- **Requirement:** Must be published for API access
- **Access:** Via GraphQL queries
- **Location:** Channel management → [Channel] → List of headless items

### GraphQL Field Naming
- **Format:** `[namespace][name]` (camelCase)
- **Example:** "Article.Article" → `articleArticle`
- **Field names:** Match content type field code names (usually lowercase)
- **String fields:** Query directly (e.g., `title`, `body`)

### API Key
- **Purpose:** Authenticate requests to GraphQL endpoint
- **Location:** Channel management → [Channel] → API keys
- **Usage:** Include as `Authorization: Bearer [api-key]` header
- **Security:** Keep secure, regenerate if compromised

---

## Quick Reference

### GraphQL Endpoint
```
POST http://localhost:43907/graphql/[channel-identifier]
```

### Required Headers
```
Content-Type: application/json
Authorization: Bearer [api-key]
```

### Basic Query Template
```json
{
  "query": "{ [contentTypeField] { [field1] [field2] } }"
}
```

### Channel Identifier Location
**Channel management → [Your Channel] → General tab → GraphQL API endpoint**

### API Key Location
**Channel management → [Your Channel] → API keys tab**

### Content Type Field Names
**Configuration → Content types → [Content Type] → Fields tab**

### Common GraphQL Queries

**Get All Articles:**
```json
{
  "query": "{ articleArticle { title body } }"
}
```

**Get Articles with Collection:**
```json
{
  "query": "{ articleArticle { nodes { title body } } }"
}
```

**Get Article with Filter:**
```json
{
  "query": "{ articleArticle(where: { title: { eq: \"My Title\" } }) { title body } }"
}
```

### PowerShell Test Command
```powershell
$headers = @{
    "Content-Type" = "application/json"
    "Authorization" = "Bearer your-api-key-here"
}

$body = @{
    query = "{ articleArticle { title body } }"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:43907/graphql/ca81ab98-d0fa-44b4-b6e8-4b34b34cb8f9" `
    -Method Post `
    -Body $body `
    -Headers $headers
```

### Angular Environment Configuration
```typescript
export const environment = {
  production: false,
  kenticoApiUrl: 'http://localhost:43907/graphql/[channel-identifier]',
  kenticoApiKey: '[your-api-key]'
};
```

---

## Conclusion

This documentation covers the complete setup process for Kentico Xperience Headless CMS integration with Angular. The setup enables:

1. **Content Management:** Via Kentico admin interface
2. **Content Delivery:** Via GraphQL API
3. **Frontend Integration:** Angular application consuming the API

**Next Steps:**
- Create more content types and fields as needed
- Implement filtering and pagination in GraphQL queries
- Add error handling and retry logic
- Implement caching strategies
- Set up production environment configurations

**Support Resources:**
- [Kentico Xperience Documentation](https://docs.xperience.io/)
- [GraphQL Documentation](https://graphql.org/)
- [Angular Documentation](https://angular.io/docs)

---

*Documentation created: February 2026*
*Kentico Xperience Version: 31.1.2*
*Angular Version: 21*
*.NET Version: 8.0*
