namespace ThriveERP.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Common;
using ThriveERP.Infrastructure.Data;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ThriveErpDbContext _context;

    public Repository(ThriveErpDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) => await _context.Set<T>().FindAsync(new object[] { id }, ct);
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) => await _context.Set<T>().ToListAsync(ct);
    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _context.Set<T>().AddAsync(entity, ct);
        return entity;
    }
    public void Update(T entity) => _context.Set<T>().Update(entity);
    public void Delete(T entity) => _context.Set<T>().Remove(entity);
}
