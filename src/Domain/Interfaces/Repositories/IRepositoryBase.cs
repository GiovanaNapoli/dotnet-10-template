using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories
{
    public interface IRepositoryBase<T> where T : class
    {
        // Create
        Task<T> AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        // Read
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);

        // Update — EF Core só marca como modificado, não precisa retornar nem ser async
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);

        // Delete — mesma lógica, EF Core só marca para remoção
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        Task DeleteAsync(T entity, CancellationToken ct = default);
        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    }
}