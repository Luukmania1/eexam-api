var builder = WebApplication.CreateBuilder(args);

// Add services for JWT Auth
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes("YourSuperSecretKey123!"))
        };
    });

var app = builder.Build();

app.MapGet("/", () => "API is running on Railway!");
app.MapGet("/health", () => "OK");

// Add this for JWT to work
app.UseAuthentication();
app.UseAuthorization();

app.Run("http://0.0.0.0:8080");