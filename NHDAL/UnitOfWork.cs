using NHDAL.Interfaces;
using NHibernate;
using NHibernate.Persister.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NHDAL
{
    /// <summary>
    /// A unit of work implementation built around NHibernate <see cref="ISession"/>,
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

        public UnitOfWork(ISession session)
        {
            _session = session;
            _session.FlushMode = FlushMode.Commit;
            _transaction = _session.BeginTransaction();
        }
        #region [Transaction]
        public void Commit()
        {
            if (_transaction?.IsActive == true)
                _transaction.Commit();
        }
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
        #endregion
        #region [Basic]
        public ISQLQuery CreateSQLQuery(string sql)
            => _session.CreateSQLQuery(sql);
        public IQueryable<TEntity> Query<TEntity>() where TEntity : class
            => _session.Query<TEntity>();
        public IQueryOver<TEntity, TEntity> QueryOver<TEntity>() where TEntity : class
            => _session.QueryOver<TEntity>();
        public bool Contains(object obj)
            => _session.Contains(obj);
        #endregion
        #region [CRUD]
        public void Delete(object obj)
            => _session.Delete(obj);
        public async Task DeleteAsync(object obj, CancellationToken cancellationToken = default)
            => await _session.DeleteAsync(obj, cancellationToken)
                             .ConfigureAwait(false);
        public async Task DeleteManyAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
        {
            foreach (var item in entities)
                await DeleteAsync(item, cancellationToken).ConfigureAwait(false);
        }
        public TEntity Merge<TEntity>(TEntity entity) where TEntity : class
        {
            // https://stackoverflow.com/questions/7475363/differences-among-save-update-saveorupdate-merge-methods-in-session
            try
            {
                _session.SaveOrUpdate(entity);
            }
            catch (NonUniqueObjectException)
            {
                // TODO: Reconcile on stale object?
                return _session.Merge(entity);
            }

            return entity;
        }
        public IEnumerable<TEntity> MergeMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            var result = new List<TEntity>();

            foreach (TEntity item in entities)
                result.Add(Merge(item));

            return result;
        }
        public async Task<TEnity> MergeAsync<TEnity>(TEnity entity, CancellationToken cancellationToken = default) where TEnity : class
        {
            try
            {
                await _session.SaveOrUpdateAsync(entity, cancellationToken)
                              .ConfigureAwait(false);
            }
            catch (NonUniqueObjectException)
            {
                // TODO: Reconcile entity / return entity from Merge (if needed)
                await _session.LockAsync(entity, LockMode.None, cancellationToken)
                              .ConfigureAwait(false);
                // return await _session.MergeAsync(entity, cancellationToken)
                //                      .ConfigureAwait(false);
            }

            return entity;
        }
        public async Task<IEnumerable<TEntity>> MergeManyAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var result = new List<TEntity>();

            foreach (TEntity item in entities)
                result.Add(await MergeAsync(item, cancellationToken).ConfigureAwait(false));

            return result;
        }
        public async Task<TEntity> SaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
        {
            await _session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);

            return entity;
        }
        public async Task<TEntity> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
        {
            await _session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

            return entity;
        }
        public IEntityPersister GetPersister<TEntity>() where TEntity : class
        {
            return _session.GetSessionImplementation()
                           .Factory
                           .TryGetEntityPersister(typeof(TEntity).FullName);
        }
        #endregion
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
