using DocumentHub.Core.Entities;
using DocumentHub.Core.Interfaces;

namespace DocumentHub.Core.UseCases.Users;

public record GetMembersQuery(Guid CallerTenantId);

public record MemberDto(Guid Id, string Email, string Role, DateTime CreatedAt);

public class GetMembersHandler(IRepository<User> users)
{
    public async Task<List<MemberDto>> HandleAsync(GetMembersQuery query, CancellationToken cancellationToken = default)
    {
        var members = await users.ListAsync(u => u.TenantId == query.CallerTenantId, cancellationToken);
        return members
            .OrderBy(u => u.CreatedAt)
            .Select(u => new MemberDto(u.Id, u.Email, u.Role.ToString(), u.CreatedAt))
            .ToList();
    }
}
