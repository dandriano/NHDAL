using Microsoft.Extensions.DependencyInjection;
using NHDAL.Tests.Mocks.Entities;
using NUnit.Framework;
using System.Linq;

namespace NHDAL.Tests
{
    [TestFixture]
    internal class UOWTests : RegistrarBase
    {
        private UnitOfWorkFactory _db;

        [SetUp]
        public void SetUp()
        {
            _db = _serviceProvider.GetRequiredService<UnitOfWorkFactory>();
        }

        [Test]
        public void UOWTest()
        {
            using var ctx = _db.OpenUnitOfWork();
            var users = ctx.Query<User>().ToList();

            Assert.That(users, Is.Empty);
        }
    }
}
