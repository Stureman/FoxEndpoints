using Asp.Versioning;
using FoxEndpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add authentication and authorization
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // For testing purposes, we'll use a simple bearer token validation
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // For testing: accept any token that starts with "test-token"
                var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                if (token.StartsWith("test-token"))
                {
                    // Create a test identity
                    var claims = new[]
                    {
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "TestUser"),
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")
                    };
                    var identity = new System.Security.Claims.ClaimsIdentity(claims, "Bearer");
                    context.Principal = new System.Security.Claims.ClaimsPrincipal(identity);
                    context.Success();
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Create API version set for FoxEndpoints
var apiVersionSet = builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("X-Api-Version")
        );
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map FoxEndpoints
// Option 1: No global authorization (endpoints opt-in individually via .RequireAuthorization())
app.UseFoxEndpoints().RequireAuthorization();

// Option 2: Global authorization (ALL endpoints require auth - uncomment to enable)
// app.UseFoxEndpoints().RequireAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}