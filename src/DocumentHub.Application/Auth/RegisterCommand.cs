using DocumentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentHub.Application.Auth;

public record RegisterCommand(string CompanyName, string Email, string Password);
public record RegisterResult(string Token);

public class RegisterHandler(IAppDbContext db, ITokenService tokenService, IPasswordHasher passwordHasher)
{
    public async Task<RegisterResult?> HandleAsync(RegisterCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.CompanyName) ||
            string.IsNullOrWhiteSpace(command.Email) ||
            string.IsNullOrWhiteSpace(command.Password))
            return null;

        var email = command.Email.ToLower();

        var tenant = new Tenant { Id = Guid.NewGuid(), Name = command.CompanyName, CreatedAt = DateTime.UtcNow };
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = email,
            PasswordHash = passwordHasher.Hash(command.Password),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        db.Tenants.Add(tenant);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return new RegisterResult(tokenService.GenerateToken(user));
    }
}
