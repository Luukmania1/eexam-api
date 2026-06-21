using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = "super_secret_key_12345_change_later";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key
    });

builder.Services.AddAuthorization();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => "OK");

app.MapPost("/api/auth/register", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        return Results.BadRequest(new { error = "username and password required" });
    var token = GenerateToken(username, jwtKey);
    return Results.Ok(new { token, username });
});

app.Run("http://0.0.0.0:" + (Environment.GetEnvironmentVariable("PORT") ?? "8080"));

string GenerateToken(string username, string jwtKey)
{
    var handler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(jwtKey);
    var token = handler.CreateToken(new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
        Expires = DateTime.UtcNow.AddHours(24),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    });
    return handler.WriteToken(token);
}