using DocumentHub.Domain.Entities;

namespace DocumentHub.Application;

public interface IAppDbContext
{
    Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
