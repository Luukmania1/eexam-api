using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// Temporary test endpoints for JWT
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
string GenerateJwtToken(string username)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKey123!"));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        claims: new[] { new System.Security.Claims.Claim("username", username) },
        expires: DateTime.Now.AddDays(7),
        signingCredentials: creds);
    
    return new System.IdentityModel.Tokens.Jwt.Jwt