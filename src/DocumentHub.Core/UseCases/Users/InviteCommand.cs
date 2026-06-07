using DocumentHub.Core.Entities;
using DocumentHub.Core.Exceptions;
using DocumentHub.Core.Interfaces;

namespace DocumentHub.Core.UseCases.Users;

public record InviteCommand(string Email, string Password, Guid CallerTenantId, string CallerRole);

public record InviteResult(Guid Id);

public class InviteHandler(IRepository<User> users, IPasswordHasher hasher)
{
    public async Task<InviteResult> HandleAsync(InviteCommand command, CancellationToken cancellationToken = default)
    {
        if (command.CallerRole != nameof(UserRole.Admin))
            throw new ForbiddenException("Only Admins can invite members.");

        var existing = await users.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        if (existing != null)
            throw new DuplicateEmailException();

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = command.CallerTenantId,
            Email = command.Email,
            PasswordHash = hasher.Hash(command.Password),
            Role = UserRole.Member,
            CreatedAt = DateTime.UtcNow
        };

        await users.AddAsync(user, cancellationToken);
        return new InviteResult(user.Id);
    }
}
