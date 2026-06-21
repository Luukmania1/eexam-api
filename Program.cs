using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// JWT Secret - same as Railway env variable
var jwtKey = builder.Configuration["Jwt:Key"] ?? "this_is_a_super_secret_key_change_this_in_production_12345";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Health check - GET works in browser
app.MapGet("/health", () => "OK");

// Register endpoint - reads from query params
app.MapPost("/api/auth/register", (HttpRequest req) =>
{
    var username = req.Query["username"].ToString();
    var password = req.Query["password"].ToString();
    
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        return Results.BadRequest(new { error = "username and password required" });
    
    var token = GenerateJwtToken(username, jwtKey);
    return Results.Ok(new { token, username });
});

// Login endpoint - reads from query params  
app.MapPost("/api/auth/login", (HttpRequest req) =>
{
    var username = req.Query["username"].ToString();
    var password = req.Query["password"].ToString();
    
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        return Results.BadRequest(new { error = "username and password required" });
    
    var token = GenerateJwtToken(username, jwtKey);
    return Results.Ok(new { token, username });
});

app.Run();

// JWT Token generator function
string GenerateJwtToken(string username,