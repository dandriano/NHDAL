using Microsoft.Extensions.DependencyInjection;
using NHDAL.Tests.Mocks;
using NHDAL.Tests.Mocks.Entities;
using NHibernate;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        public void Basic_BuildSchemaTest()
        {
            if (!_db.TryOpenUnitOfWork(out var ctx))
            {
                Assert.Fail();
                return; 
            }
            using (ctx)
            {
                Assert.That(ctx.Query<User>().ToList(), Has.Count.Zero);
            }
        }
        [Test]
        public void Basic_InsertTest()
        {
            (var users, var posts, var comments) = MocksHelper.GenerateMockDomainData();

            if (!_db.TryOpenUnitOfWork(out var ctx))
            {
                Assert.Fail();
                return; 
            }

            using (ctx)
            {
                ctx.MergeMany(users);
                ctx.Commit();
            }

            Thread.Sleep(500);

            if (!_db.TryOpenUnitOfWork(out ctx))
            {
                Assert.Fail();
                return;
            }

            Assert.Multiple(() =>
            {
                Assert.That(ctx.Query<User>().ToList(), Has.Count.EqualTo(users.Count));
                Assert.That(ctx.Query<Post>().ToList(), Has.Count.EqualTo(posts.Count));
                Assert.That(ctx.Query<Comment>().ToList(), Has.Count.EqualTo(comments.Count));
            });
        }
        [Test]
        public void Basic_VersionChanged_ReturnsFromCash_Test()
        {
            (var users, _, _) = MocksHelper.GenerateMockDomainData();
            if (!_db.TryOpenUnitOfWork(out var ctx))
            {
                Assert.Fail();
                return; 
            }

            using (ctx)
            {
                users = (List<User>)ctx.MergeMany(users);
                ctx.Commit();
            }

            Thread.Sleep(1500);

            users[0].Name = "Incognito";
            if (!_db.TryOpenUnitOfWork(out ctx))
            {
                Assert.Fail();
                return; 
            }
            using (ctx)
            {
                var detachedUser = ctx.Merge(users[0]);
                var targetUser = ctx.Query<User>().Single(u => users[0].Id == u.Id);
                Assert.Multiple(() =>
                {
                    Assert.That(detachedUser.Name, Is.EqualTo(targetUser.Name));
                    Assert.That(detachedUser.Timestamp, Is.EqualTo(targetUser.Timestamp));
                });
            }
        }
        [Test]
        public void Basic_VersionChanged_OutsideOfUOW_Test()
        {
            (var users, _, _) = MocksHelper.GenerateMockDomainData();
            if (!_db.TryOpenUnitOfWork(out var ctx))
            {
                Assert.Fail();
                return; 
            }
            using (ctx)
            {
                users = (List<User>)ctx.MergeMany(users);
                ctx.Commit();
            }

            Thread.Sleep(1500);
            users[0].Name = "Incognito";
            
            if (!_db.TryOpenUnitOfWork(out ctx))
            {
                Assert.Fail();
                return; 
            }
            using (ctx)
            {
                var targetUser = ctx.Query<User>().Single(u => users[0].Id == u.Id);
                var detachedUser = ctx.Merge(users[0]);
                Assert.Multiple(() =>
                {
                    Assert.That(detachedUser.Name, Is.EqualTo(targetUser.Name));
                    Assert.That(detachedUser.Timestamp, Is.EqualTo(targetUser.Timestamp));
                });
            }
        }
        [TestCase(5)]
        public async Task OptimisticConcurrency_WithinBoundariesOfUOW_Test(int concurrencyLimit)
        {
            (var users, _, _) = MocksHelper.GenerateMockDomainData();
            var targetId = users[^1].Id;
            
            if (!_db.TryOpenUnitOfWork(out var ctx))
            {
                Assert.Fail();
                return;
            }
            using (ctx)
            {
                await ctx.MergeManyAsync(users);
                await ctx.CommitAsync();
            }

            var s = DateTime.Now;
            var c = Enumerable
                .Range(1, concurrencyLimit)
                .Select(i =>
                    Task.Run(() =>
                    {
                        // optimistic concurrency in the example below 
                        // within the bounds of the ISession 
                        var success = true;

                        Thread.Sleep(new Random().Next(1, concurrencyLimit) * 100);
                        TestContext.WriteLine($"{DateTime.Now.Subtract(s).TotalMilliseconds:f0} ms\tuser:{i}\tstart");
                        using var ctx = _db.OpenUnitOfWork();
                        var toEdit = ctx.Query<User>().Single(u => u.Id == targetId);

                        Thread.Sleep(new Random().Next(1, concurrencyLimit ^ 2) * 1000);
                        TestContext.WriteLine($"{DateTime.Now.Subtract(s).TotalMilliseconds:f0} ms\tuser:{i}\tflush");
                        toEdit.Name = $"HELLO WORLD FROM user{i}";

                        try
                        {
                            ctx.Merge(toEdit);
                            ctx.Commit();
                        }
                        catch (StaleObjectStateException)
                        {
                            success = false;
                        }

                        TestContext.WriteLine($"{DateTime.Now.Subtract(s).TotalMilliseconds:f0} ms\tuser:{i}\tend succeeded:{success}");
                        return success;
                    })
                );

            var results = await Task.WhenAll(c);
            Assert.That(results.Count(succeeded => succeeded), Is.EqualTo(1));
        }
        [TestCase(5)]
        public async Task OptimisticConcurrency_OutsideBoundariesOfUOW_Test(int concurrencyLimit)
        {
            (var users, _, _) = MocksHelper.GenerateMockDomainData();
            var targetId = users[^1].Id;

            if (!_db.TryOpenUnitOfWork(out var ctx))
            {
                Assert.Fail();
                return;
            }
            using (ctx)
            {
                await ctx.MergeManyAsync(users);
                await ctx.CommitAsync();
            }

            var s = DateTime.Now;
            var c = Enumerable
                .Range(1, concurrencyLimit)
                .Select(i =>
                    Task.Run(() =>
                    {
                        // optimistic concurrency in the example below 
                        // outside of the bounds of the ISession 
                        var success = true;

                        Thread.Sleep(new Random().Next(1, concurrencyLimit) * 100);
                        TestContext.WriteLine($"{DateTime.Now.Subtract(s).TotalMilliseconds:f0} ms\tuser:{i}\tstart");
                        var toEdit = User.Nobody;
                        using (var ctx = _db.OpenUnitOfWork())
                        {
                            toEdit = ctx.Query<User>().Single(u => u.Id == targetId);
                        }

                        Thread.Sleep(new Random().Next(1, concurrencyLimit ^ 2) * 1000);
                        TestContext.WriteLine($"{DateTime.Now.Subtract(s).TotalMilliseconds:f0} ms\tuser:{i}\tflush");
                        toEdit.Name = $"HELLO WORLD FROM user{i}";

                        try
                        {
                            using var ctx = _db.OpenUnitOfWork();
                            ctx.Merge(toEdit);
                            ctx.Commit();
                        }
                        catch (StaleObjectStateException)
                        {
                            success = false;
                        }

                        TestContext.WriteLine($"{DateTime.Now.Subtract(s).TotalMilliseconds:f0} ms\tuser:{i}\tend succeeded:{success}");
                        return success;
                    })
                );

            var results = await Task.WhenAll(c);
            Assert.That(results.Count(succeeded => succeeded), Is.EqualTo(1));
        }
    }
}
