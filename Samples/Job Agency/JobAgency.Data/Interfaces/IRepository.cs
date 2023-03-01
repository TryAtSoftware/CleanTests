namespace JobAgency.Data.Interfaces;

using System.Linq.Expressions;
using JobAgency.Models.Interfaces;

public interface IRepository<TEntity>
    where TEntity : IIdentifiable
{
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken);
    Task<TEntity> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken);
    Task<bool> UpdateField<TValue>(Guid id, Expression<Func<TEntity, TValue>> fieldExpression, TValue newValue, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}