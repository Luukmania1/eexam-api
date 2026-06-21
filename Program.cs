using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services for JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("YourSuperSecretKey123!"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Add these 2 lines for JWT to work
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "API is running on Railway!");

app.MapGet("/health", () => "OK");

// Test endpoints for JWT
app.MapPost("/api/auth/register", (string username, string password) =>
{
    var token = GenerateJwtToken(username);
    return Results.Ok(new { token, username });
});

app.MapPost("/api/auth/login", (string username, string password) =>
{
    var token = GenerateJwtToken(username);
    return Results.Ok(new { token, username });
});

// Helper function to create JWT token
string GenerateJwt