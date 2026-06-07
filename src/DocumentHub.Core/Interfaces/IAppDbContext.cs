using DocumentHub.Core.Entities;

namespace DocumentHub.Core.Interfaces;

public interface IAppDbContext
{
    Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
