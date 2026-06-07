using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DocumentHub.Application.Auth;
using DocumentHub.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DocumentHub.Infrastructure.Auth;

public class JwtTokenService(IConfiguration configuration) : ITokenService
{
    private const string UserIdClaim = "userId";
    private const string TenantIdClaim = "tenantId";
    private const string RoleClaim = "role";

    public string GenerateToken(User user)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = configuration["Jwt:Issuer"] ?? "DocumentHub";

        var claims = new[]
        {
            new Claim(UserIdClaim, user.Id.ToString()),
            new Claim(TenantIdClaim, user.TenantId.ToString()),
            new Claim(RoleClaim, user.Role.ToString())
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
}
