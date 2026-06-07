using DocumentHub.Core.Entities;
using DocumentHub.Core.Exceptions;
using DocumentHub.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocumentHub.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    internal DbSet<Tenant> Tenants => Set<Tenant>();
    internal DbSet<User> Users => Set<User>();

    public Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        Tenants.Add(tenant);
        return Task.CompletedTask;
    }

    public Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }

    async Task IAppDbContext.SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
        {
            throw new DuplicateEmailException();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
