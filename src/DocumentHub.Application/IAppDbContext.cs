using DocumentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentHub.Application;

public interface IAppDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
