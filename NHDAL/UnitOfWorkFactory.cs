using Microsoft.Extensions.Options;
using NHibernate;
using System.Diagnostics.CodeAnalysis;

namespace NHDAL
{
    /// <summary>
    /// Threread-safe factory for creating <see cref="IUnitOfWork"/> instances 
    /// backed by NHibernate sessions. 
    /// </summary>
    public class UnitOfWorkFactory
    {
        private readonly UnitOfWorkFactoryOptions _options;
        private readonly ISessionFactory _sessionFactory;

        public UnitOfWorkFactory(IOptions<UnitOfWorkFactoryOptions> options)
        {
            _options = options.Value;
            _sessionFactory = ConfigurationHelper.CreateConfiguration(_options)
                                                 .BuildSessionFactory();
        }
        public IUnitOfWork OpenUnitOfWork()
            => new UnitOfWork(_sessionFactory.OpenSession());

        public bool TryOpenUnitOfWork([MaybeNullWhen(false)] out IUnitOfWork pc)
        {
            pc = null;

            try
            {
                pc = OpenUnitOfWork();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
