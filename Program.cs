var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "API is running on Railway!");
app.MapGet("/health", () => "OK");

app.Run("http://0.0.0.0:8080");