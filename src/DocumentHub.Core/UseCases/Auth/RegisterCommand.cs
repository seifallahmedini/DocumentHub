using DocumentHub.Core.Entities;
using DocumentHub.Core.Exceptions;
using DocumentHub.Core.Interfaces;

namespace DocumentHub.Core.UseCases.Auth;

public record RegisterCommand(string CompanyName, string Email, string Password);
public record RegisterResult(string Token);

public class RegisterHandler(IAppDbContext db, ITokenService tokenService, IPasswordHasher passwordHasher)
{
    public async Task<RegisterResult> HandleAsync(RegisterCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.CompanyName) ||
            string.IsNullOrWhiteSpace(command.Email) ||
            string.IsNullOrWhiteSpace(command.Password))
            throw new RegistrationValidationException("CompanyName, Email, and Password are required.");

        var email = command.Email.Trim().ToLower();

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

        await db.AddTenantAsync(tenant);
        await db.AddUserAsync(user);
        await db.SaveChangesAsync();

        return new RegisterResult(tokenService.GenerateToken(user.Id, user.TenantId, user.Role.ToString()));
    }
}
