using System.Linq.Expressions;
using DocumentHub.Core.Interfaces;
using DocumentHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DocumentHub.Infrastructure.Data;

public class EfRepository<T>(AppDbContext context) : IRepository<T> where T : class
{
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        context.Set<T>().Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        context.Set<T>().Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await context.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);
}
