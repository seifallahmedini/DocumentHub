using DocumentHub.Core.Entities;
using DocumentHub.Core.Exceptions;
using DocumentHub.Core.Interfaces;

namespace DocumentHub.Core.UseCases.Auth;

public record RegisterCommand(string CompanyName, string Email, string Password);
public record RegisterResult(string Token);

public class RegisterHandler(IRepository<Tenant> tenants, ITokenService tokenService, IPasswordHasher passwordHasher)
{
    public async Task<RegisterResult> HandleAsync(RegisterCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.CompanyName) ||
            string.IsNullOrWhiteSpace(command.Email) ||
            string.IsNullOrWhiteSpace(command.Password))
            throw new RegistrationValidationException("CompanyName, Email, and Password are required.");

        var email = command.Email.Trim().ToLower();

        var tenant = new Tenant { Id = Guid.NewGuid(), Name = command.CompanyName, CreatedAt = DateTime.UtcNow };
        tenant.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = email,
            PasswordHash = passwordHasher.Hash(command.Password),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        });

        await tenants.AddAsync(tenant);  // EF cascade-inserts User atomically

        var admin = tenant.Users.First();
        return new RegisterResult(tokenService.GenerateToken(admin.Id, admin.TenantId, admin.Role.ToString()));
    }
}
