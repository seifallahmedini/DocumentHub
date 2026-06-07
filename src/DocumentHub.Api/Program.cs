using System.Text;
using DocumentHub.Application.Auth;
using DocumentHub.Infrastructure;
using DocumentHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key must be configured. Set it in appsettings or environment variables.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DocumentHub";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=documenthub.db";

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<RegisterHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/register", async (RegisterRequest request, RegisterHandler handler) =>
{
    if (string.IsNullOrWhiteSpace(request.CompanyName) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new { error = "CompanyName, Email, and Password are required." });

    try
    {
        var result = await handler.HandleAsync(
            new RegisterCommand(request.CompanyName, request.Email, request.Password));

        return result is null
            ? Results.BadRequest(new { error = "Invalid registration data." })
            : Results.Ok(new { token = result.Token });
    }
    catch (DbUpdateException)
    {
        return Results.Conflict(new { error = "A user with this email already exists." });
    }
});

app.Run();

record RegisterRequest(string CompanyName, string Email, string Password);

public partial class Program { }
