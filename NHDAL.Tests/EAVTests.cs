using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace NHDAL.Tests
{
    [TestFixture]
    internal class EAVTests : RegistrarBase
    {
        private UnitOfWorkFactory _db;

        [SetUp]
        public void SetUp()
        {
            _db = _serviceProvider.GetRequiredService<UnitOfWorkFactory>();
            _db.BuildSchema();
        }
    }
}