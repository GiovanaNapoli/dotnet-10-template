using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Domain.Interfaces;
using Infrastructure.Connector;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Domain.Interfaces.Repositories;

namespace Infrastructure.Repositories
{
    public class RepositoryBase<T> : DbConnector, IRepositoryBase<T> where T : class
    {
        public readonly ApplicationDbContext _context;

        public RepositoryBase(ApplicationDbContext context, IConfiguration configuration) : base(configuration)
        {
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        // Create
        public async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            await _context.Set<T>().AddAsync(entity, ct);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            await _context.Set<T>().AddRangeAsync(entities, ct);
        }

        // Read
        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var value = await _context.Set<T>().FindAsync(new object[] { id }, ct);
            return value;
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<T>().ToListAsync(ct);
        }

        // Update
        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _context.Set<T>().UpdateRange(entities);
        }

        // Delete
        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public Task DeleteAsync(T entity, CancellationToken ct = default)
        {
            _context.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            _context.Set<T>().RemoveRange(entities);
            return Task.CompletedTask;
        }
    }
}