using NHibernate;
using NHibernate.Persister.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NHDAL.Interfaces
{
    /// <summary>
    /// TODO: Comments
    /// </summary>
    /// <remarks>
    /// Just more strict interace than <see cref="ISession"/>
    /// </remarks>
    public interface IUnitOfWork : IDisposable
    {
        IEntityPersister GetPersister<TEntity>() where TEntity : class;
        ISQLQuery CreateSQLQuery(string sql);
        IQueryable<TEntity> Query<TEntity>() where TEntity : class;
        bool Contains(object obj);
        void Delete(object obj);
        Task DeleteAsync(object obj, CancellationToken cancellationToken = default);
        Task DeleteManyAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default);
        TEntity Merge<TEntity>(TEntity entity) where TEntity : class;
        IEnumerable<TEntity> MergeMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        Task<TEntity> MergeAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class;
        Task<IEnumerable<TEntity>> MergeManyAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class;
        void Commit();
        void Rollback();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
