using System.Linq.Expressions;

namespace WebAppExam.Domain.Common
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default);

        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<List<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(List<T> entities, CancellationToken cancellationToken = default);

        void Update(T entity);
        void UpdateRange(List<T> entities);

        void Remove(T entity);

        IQueryable<T> Query();
        Task<List<T>> ToListAsync(IQueryable<T> query, CancellationToken cancellationToken = default);
    }
}
