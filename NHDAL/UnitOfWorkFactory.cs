using Microsoft.Extensions.Options;
using NHDAL.Interfaces;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using System.Diagnostics.CodeAnalysis;

namespace NHDAL
{
    /// <summary>
    /// Threread-safe factory for creating <see cref="IUnitOfWork"/> instances 
    /// backed by NHibernate sessions. 
    /// </summary>
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly UnitOfWorkFactoryOptions _options;
        private readonly ISessionFactory _sessionFactory;

        public UnitOfWorkFactory(IOptions<UnitOfWorkFactoryOptions> options)
        {
            _options = options.Value;
            _sessionFactory = ConfigurationExtensions.CreateConfiguration(_options)
                                                     .BuildSessionFactory();
        }
        public IUnitOfWork OpenUnitOfWork()
            => new UnitOfWork(_sessionFactory.OpenSession());

        public bool TryOpenUnitOfWork([MaybeNullWhen(false)] out IUnitOfWork pc)
        {
            try
            {
                pc = OpenUnitOfWork();
                return true;
            }
            catch
            {
                pc = null;
                return false;
            }
        }
        public void BuildSchema()
        {
            new SchemaExport(ConfigurationExtensions.CreateConfiguration(_options)).Create(false, true);
        }
    }
}
