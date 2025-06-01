using NHibernate;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NHDAL
{
    /// <summary>
    /// A unit of work implementation built on NHibernate,
    /// leveraging its LINQ capabilities via <see cref="Query"/>. 
    /// </summary>
    /// <remarks>
    /// It manages a session (<see cref="ISession"/>) and (partially) a transaction (<see cref="ITransaction"/>),
    /// with the session configured to flush changes only on commit (<see cref="FlushMode.Commit"/>). 
    /// Key methods related to reattaching detached objects include <see cref="Merge"/> and <see cref="MergeAsync"/>.
    /// The class aims to handle both new and detached entities within a transactional boundary.
    /// </remarks>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ISession _session;
        private readonly ITransaction _transaction;
        public ISessionImplementor Implementation => _session.GetSessionImplementation();
        public IPersistenceContext PersistenceContext => Implementation.PersistenceContext;

        public UnitOfWork(ISession session)
        {
            _session = session;
            _session.FlushMode = FlushMode.Commit;
            _transaction = _session.BeginTransaction();
        }
        public void Commit()
            => _transaction.Commit();
        public void Rollback()
        {
            if (_transaction?.IsActive == true)
                _transaction.Rollback();
        }
        public async Task CommitAsync()
        {
            if (_transaction?.IsActive == true)
                await _transaction.CommitAsync().ConfigureAwait(false);
        }
        public async Task RollbackAsync()
        {
            if (_transaction?.IsActive == true)
                await _transaction.RollbackAsync().ConfigureAwait(false);
        }
        public IQueryable<TEntity> Query<TEntity>() where TEntity : class
            => _session.Query<TEntity>();
        public IQueryOver<TEntity, TEntity> QueryOver<TEntity>() where TEntity : class
            => _session.QueryOver<TEntity>();
        public async Task<List<TEntity>> MergeManyAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var result = new List<TEntity>();

            foreach (TEntity item in entities)
                result.Add(await MergeAsync(item, cancellationToken).ConfigureAwait(false));

            return result;
        }
        public async Task DeleteManyAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
        {
            foreach (var item in entities)
                await DeleteAsync(item, cancellationToken).ConfigureAwait(false);
        }
        public bool Contains(object obj)
            => _session.Contains(obj);
        public void Delete(object obj)
            => _session.Delete(obj);
        public async Task DeleteAsync(object obj, CancellationToken cancellationToken = default)
            => await _session.DeleteAsync(obj, cancellationToken).ConfigureAwait(false);
        public T Merge<T>(T entity) where T : class
        {
            try
            {
                _session.SaveOrUpdate(entity);

                return entity;
            }
            catch (NonUniqueObjectException)
            {
                return _session.Merge(entity);
            }
        }
        public async Task<T> MergeAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                await _session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);

                return entity;
            }
            catch (NonUniqueObjectException)
            {
                return await _session.MergeAsync(entity, cancellationToken).ConfigureAwait(false);
            }

        }
        public async Task<T> SaveAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            await _session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);

            return entity;
        }
        public async Task<T> UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            await _session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

            return entity;
        }
        public IEntityPersister GetPersister<TEntity>() where TEntity : class
            => Implementation.Factory.TryGetEntityPersister(typeof(TEntity).FullName);
        public ISQLQuery CreateSQLQuery(string sql)
            => _session.CreateSQLQuery(sql);
        public void Dispose()
        {
            try
            {
                Rollback();
            }
            finally
            {
                _session.Dispose();
            }
        }
    }
}
