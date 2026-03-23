using Domain.Entities.Common;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RepositoryBase<T> : IRepositoryBase<T> where T : BaseAuditableEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public RepositoryBase(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
        // Removido: QueryTrackingBehavior.NoTracking global
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => await _dbSet.AddRangeAsync(entities, ct);

    // AsNoTracking apenas nas queries de leitura — não afeta Update/Delete
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .ToListAsync(ct);

    // Update e Delete sem AsNoTracking — EF Core precisa rastrear para persistir
    public void Update(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void UpdateRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(T entity)
        => _dbSet.Remove(entity);

    public void DeleteRange(IEnumerable<T> entities)
        => _dbSet.RemoveRange(entities);

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        _dbSet.Remove(entity);
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        _dbSet.RemoveRange(entities);
    }
}