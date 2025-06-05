using Microsoft.Extensions.DependencyInjection;
using NHDAL.Tests.Mocks.Entities;
using NUnit.Framework;
using System.Collections.Generic;
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
            _db.BuildSchema();
        }
        [Test]
        public void BuildSchemaTest()
        {
            using var ctx = _db.OpenUnitOfWork();
            var users = ctx.Query<User>().ToList();

            Assert.That(users, Is.Empty);
        }
        [Test]
        public void InsertTest()
        {
            var users = new List<User>()
            {
                new User() { Name = "AlrigthAlrightAlright" },
                new User() { Name = "SillySandler" },
                new User() { Name = "PerspectiveDiCaprio" },
            };

            using (var ctx1 = _db.OpenUnitOfWork())
            {
                ctx1.MergeMany(users);
                ctx1.Commit();
            }

            using var ctx2 = _db.OpenUnitOfWork();
            Assert.That(ctx2.Query<User>().ToList(), Has.Count.EqualTo(users.Count));
        }
    }
}
