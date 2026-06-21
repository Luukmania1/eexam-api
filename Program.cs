using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"] ?? "super_secret_key_12345_change_in_prod";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

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

app.MapGet("/health", () => "OK");

app.MapPost("/api/auth/register", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();
    
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        return Results.BadRequest(new { error = "username and password required" });
    
    var token = GenerateJwtToken(username, jwtKey);
    return Results.Ok(new { token, username });
});

app.MapPost("/api/auth/login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();
    
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        return Results.BadRequest(new { error = "username and password required" });
    
    var token = GenerateJwtToken(username, jwtKey);
    return Results.Ok(new { token, username });
});

app.Run("http://0.0.0.0:" + (Environment.GetEnvironmentVariable("PORT") ?? "8080"));

string GenerateJwtToken(string username, string jwtKey)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(jwtKey);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
        Expires = DateTime.UtcNow.AddHours(24),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}