using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DocumentHub.Api.Data;
using DocumentHub.Api.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "documenthub-dev-secret-key-min-32-chars!!";
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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

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

app.MapPost("/auth/register", async (RegisterRequest request, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.CompanyName) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new { error = "CompanyName, Email, and Password are required." });

    var emailExists = await db.Users.AnyAsync(u => u.Email == request.Email.ToLower());
    if (emailExists)
        return Results.Conflict(new { error = "A user with this email already exists." });

    var tenant = new Tenant
    {
        Id = Guid.NewGuid(),
        Name = request.CompanyName,
        CreatedAt = DateTime.UtcNow
    };

    var user = new User
    {
        Id = Guid.NewGuid(),
        TenantId = tenant.Id,
        Email = request.Email.ToLower(),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        Role = UserRole.Admin,
        CreatedAt = DateTime.UtcNow
    };

    db.Tenants.Add(tenant);
    db.Users.Add(user);
    await db.SaveChangesAsync();

    var token = GenerateJwt(user, jwtKey, jwtIssuer);
    return Results.Ok(new { token });
});

app.Run();

static string GenerateJwt(User user, string key, string issuer)
{
    var claims = new[]
    {
        new Claim("userId", user.Id.ToString()),
        new Claim("tenantId", user.TenantId.ToString()),
        new Claim("role", user.Role.ToString())
    };

    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: issuer,
        claims: claims,
        expires: DateTime.UtcNow.AddDays(7),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

record RegisterRequest(string CompanyName, string Email, string Password);

public partial class Program { }
