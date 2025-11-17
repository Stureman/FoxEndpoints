using FoxEndpoints;
using FoxEndpoints.Tests.TestEndpoints;

var builder = WebApplication.CreateBuilder(args);

// Register test services
builder.Services.AddSingleton<ITestService, TestService>();

var app = builder.Build();

// Map all endpoints
app.UseFoxEndpoints();

app.Run();

// Make the implicit Program class accessible to WebApplicationFactory for testing
public partial class Program
{
}