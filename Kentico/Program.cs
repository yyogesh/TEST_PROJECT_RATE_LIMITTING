using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Headless;

var builder = WebApplication.CreateBuilder(args);

// Enable desired Kentico Xperience features
builder.Services.AddKentico(features =>
{
    // features.UsePageBuilder();
    // features.UseActivityTracking();
    // features.UseWebPageRouting();
    // features.UseEmailStatisticsLogging();
    // features.UseEmailMarketing();
    // Headless is included automatically, no explicit call needed
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization(); // Uncomment this

// Add both MVC and API controllers
builder.Services.AddControllers(); // This enables API controllers
builder.Services.AddControllersWithViews(); // This enables MVC

var app = builder.Build();

app.InitKentico();

app.UseStaticFiles();
app.UseCookiePolicy();
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization(); // Uncomment this

app.UseKentico();

// Map both API and MVC routes
app.MapControllers(); // This maps API controllers
app.Kentico().MapRoutes(); // This maps Kentico routes

app.MapGet("/", () => "The AcmeWeb site has not been configured yet.");

app.Run();