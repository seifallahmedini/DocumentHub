using DocumentHub.Core.Entities;
using DocumentHub.Core.Exceptions;
using DocumentHub.Core.Interfaces;

namespace DocumentHub.Core.UseCases.Users;

public record RemoveMemberCommand(Guid UserId, Guid CallerUserId, Guid CallerTenantId, string CallerRole);

public class RemoveMemberHandler(IRepository<User> users)
{
    public async Task HandleAsync(RemoveMemberCommand command, CancellationToken cancellationToken = default)
    {
        if (command.CallerRole != nameof(UserRole.Admin))
            throw new ForbiddenException("Only Admins can remove members.");

        if (command.UserId == command.CallerUserId)
            throw new InvalidOperationException("An Admin cannot remove themselves.");

        var user = await users.FirstOrDefaultAsync(
            u => u.Id == command.UserId && u.TenantId == command.CallerTenantId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User not found.");

        await users.DeleteAsync(user, cancellationToken);
    }
}
