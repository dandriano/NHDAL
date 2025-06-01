using NHibernate;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NHDAL
{
    /// <summary>
    /// TODO: Comments
    /// </summary>
    /// <remarks>
    /// Just more strict interace than <see cref="ISession"/>
    /// </remarks>
    public interface IUnitOfWork : IDisposable
    {
        ISessionImplementor Implementation { get; }
        IPersistenceContext PersistenceContext { get; }
        IEntityPersister GetPersister<TEntity>() where TEntity : class;
        ISQLQuery CreateSQLQuery(string sql);
        IQueryable<TEntity> Query<TEntity>() where TEntity : class;
        bool Contains(object obj);
        void Delete(object obj);
        Task DeleteAsync(object obj, CancellationToken cancellationToken = default);
        Task DeleteManyAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default);
        T Merge<T>(T entity) where T : class;
        Task<T> MergeAsync<T>(T entity, CancellationToken cancellationToken = default)
            where T : class;
        Task<List<TEntity>> MergeManyAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class;
        void Commit();
        void Rollback();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
