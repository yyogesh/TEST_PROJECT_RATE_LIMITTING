using SEB.FPE.Telemetry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Telemetry - Service Registration
builder.Services.AddTelemetry(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

// Telemetry Middleware - captures requests, responses, and exceptions
app.UseTelemetry();

app.UseAuthorization();

app.MapControllers();

// Home endpoint
app.MapGet("/", () => new
{
    message = "Telemetry Test App",
    description = "Test endpoints available at /api/test",
    endpoints = new[]
    {
        "GET /api/test - Simple GET request",
        "POST /api/test - POST request with body",
        "GET /api/test/error - Simulates an error",
        "GET /api/test/slow - Simulates slow request",
        "GET /api/test/query?token=test123 - Test query parameter masking"
    }
});

app.Run();
