using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NHDAL.Interfaces;
using NHDAL.Tests.Domains;
using NHDAL.Tests.Domains.EAV.Entities;
using NHDAL.Tests.Domains.Relative.Entities;
using NHibernate;
using NHibernate.Envers.Query;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NHDAL.Tests
{
    [TestFixture]
    public class UOWTests : RegistrarBase
    {
        IUnitOfWorkRunner _scopeExecutor = null!;

        [SetUp]
        public void SetUp()
        {
            var options = _serviceProvider.GetRequiredService<IOptions<UnitOfWorkFactoryOptions>>();
            new SchemaExport(ConfigurationExtensions.CreateConfiguration(options.Value)).Create(false, true);

            _scopeExecutor = _serviceProvider.GetRequiredService<IUnitOfWorkRunner>();
        }
        
        [Test]
        public void Basic_BuildSchemaTest()
        {

            var userCount = _scopeExecutor.Run(sp =>
            {
                var ctx = sp.GetRequiredService<IUnitOfWork>();

                return ctx.Query<User>().ToList().Count;
            });

            Assert.That(userCount, Is.Zero);
        }
        [Test]
        public void Basic_InsertTest()
        {
            var (users, posts, comments) = DomainsHelper.GenerateRelativeDomainData();

            _scopeExecutor.Run(sp =>
            {
                var ctx = sp.GetRequiredService<IUnitOfWork>();

                ctx.MergeMany(users);
            });

            Thread.Sleep(500);

            var (userCount, postCount, commentCount) = _scopeExecutor.Run(sp =>
            {
                var ctx = sp.GetRequiredService<IUnitOfWork>();

                var userCount = ctx.Query<User>().ToList().Count;
                var postCount = ctx.Query<Post>().ToList().Count;
                var commentCount = ctx.Query<Comment>().ToList().Count;

                return (userCount, postCount, commentCount);
            });

            Assert.Multiple(() =>
            {
                Assert.That(userCount, Is.EqualTo(users.Count));
                Assert.That(postCount, Is.EqualTo(posts.Count));
                Assert.That(commentCount, Is.EqualTo(comments.Count));
            });
        }
        [Test]
        public void Basic_InsertEAVTest()
        {
            (var types, var attributes, var records) = DomainsHelper.GenerateEAVDomainData();
            _scopeExecutor.Run(sp =>
            {
                var ctx = sp.GetRequiredService<IUnitOfWork>();

                ctx.MergeMany(types);
            });

            Thread.Sleep(500);

            var (entityCount, attributeCount, recordCount) = _scopeExecutor.Run(sp =>
            {
                var ctx = sp.GetRequiredService<IUnitOfWork>();

                var entityCount = ctx.Query<Entity>().ToList().Count;
                var attributeCount = ctx.Query<Domains.EAV.Entities.Attribute>().ToList().Count;
                var recordCount = ctx.Query<EntityRecord>().ToList().Count;

                return (entityCount, attributeCount, recordCount);
            });

            Assert.Multiple(() =>
            {
                Assert.That(entityCount, Is.EqualTo(types.Count));
                Assert.That(attributeCount, Is.EqualTo(attributes.Count));
                Assert.That(recordCount, Is.EqualTo(records.Count));
            });

            Thread.Sleep(500);

            _scopeExecutor.Run(sp =>
            {
                var ctx = sp.GetRequiredService<IUnitOfWork>();

                var entityCount = ctx.Query<Entity>().ToList().Count;
                var attributeCount = ctx.Query<Domains.EAV.Entities.Attribute>().ToList().Count;
                var recordCount = ctx.Query<EntityRecord>().ToList().Count;

                return (entityCount, attributeCount, recordCount);
            });

            Thread.Sleep(500);

            _scopeExecutor.Run(sp =>
            {
                var ctx = sp.GetRequiredService<IUnitOfWork>();

                var audit = ctx.GetAuditReader();
                var auditedRecord = records.First();

                var revFromVersion = audit.GetRevisionNumberForDate(auditedRecord.Timestamp);
                var revFromNow = audit.GetRevisionNumberForDate(DateTime.UtcNow);
                // var historyRecord = audit.Find<EntityRecord>(auditedRecord, revFromVersion);
                var historyRecord = audit.CreateQuery()
                    .ForEntitiesAtRevision<EntityRecord>(revFromVersion)
                    .Add(AuditEntity.Id().Eq(auditedRecord.Id))
                    .Single();

                Assert.Multiple(() =>
                {
                    Assert.That(revFromVersion, Is.EqualTo(revFromNow));
                    Assert.That(auditedRecord.Id, Is.EqualTo(historyRecord.Id));
                });
            });
        }
        [Test]
        public void Basic_VersionIncrement_Test()
        {
            var db = _serviceProvider.GetRequiredService<IUnitOfWorkFactory>();

            if (!db.TryOpenUnitOfWork(out var ctx))
            {
                Assert.Fail();
                return;
            }

            var user = new User { Name = "McConaughey" };
            using (ctx)
            {
                user = ctx.Merge(user);
                ctx.Commit();
            }

            var initialTimestamp = user.Timestamp;
            if (!db.TryOpenUnitOfWork(out ctx))
            {
                Assert.Fail();
                return;
            }

            using (ctx)
            {
                user = ctx.Query<User>().Single(u => u.Name == "McConaughey");
                Assert.That(initialTimestamp, Is.EqualTo(user.Timestamp));
                initialTimestamp = user.Timestamp;

                user.Name = "McConaissance";
                user = ctx.Merge(user);
                ctx.Commit();
            }

            if (!db.TryOpenUnitOfWork(out ctx))
            {
                Assert.Fail();
                return;
            }

            using (ctx)
            {
                user = ctx.Query<User>().Single(u => u.Name == "McConaissance");
                Assert.That(user.Timestamp, Is.GreaterThan(initialTimestamp));
            }
        }
        [Test]
        public void Basic_VersionNotIncrement_ReturnsFromCache_Test()
        {
            (var users, _, _) = DomainsHelper.GenerateRelativeDomainData();

            var db = _serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
            if (!db.TryOpenUnitOfWork(out var ctx))
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

            if (!db.TryOpenUnitOfWork(out ctx))
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
        public void Basic_VersionNotIncrement_Merge_Test()
        {
            (var users, _, _) = DomainsHelper.GenerateRelativeDomainData();

            var db = _serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
            if (!db.TryOpenUnitOfWork(out var ctx))
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

            if (!db.TryOpenUnitOfWork(out ctx))
            {
                Assert.Fail();
                return;
            }

            using (ctx)
            {
                var detachedUser = users[0];
                var targetUser = ctx.Query<User>().Single(u => users[0].Id == u.Id);
                Assert.Multiple(() =>
                {
                    Assert.That(detachedUser.Name, Is.Not.EqualTo(targetUser.Name));
                    Assert.That(detachedUser.Timestamp, Is.EqualTo(targetUser.Timestamp));
                });
                detachedUser = ctx.Merge(users[0]);
                Assert.Multiple(() =>
                {
                    Assert.That(detachedUser.Name, Is.EqualTo(targetUser.Name));
                    Assert.That(detachedUser.Timestamp, Is.EqualTo(targetUser.Timestamp));
                });
            }
        }
        [Test]
        public void Basic_VersionIncrement_ReturnsFromCache_Test()
        {
            (var users, _, _) = DomainsHelper.GenerateRelativeDomainData();

            var db = _serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
            if (!db.TryOpenUnitOfWork(out var ctx))
            {
                Assert.Fail();
                return;
            }

            using (ctx)
            {
                users = (List<User>)ctx.MergeMany(users);
                ctx.Commit();
            }

            var detachedUser = users[0];
            detachedUser.Name = "Incognito";
            Thread.Sleep(1500);
            if (!db.TryOpenUnitOfWork(out ctx))
            {
                Assert.Fail();
                return;
            }

            using (ctx)
            {
                var targetConcurrentUser = ctx.Query<User>()
                                              .Single(u => users[0].Id == u.Id);
                targetConcurrentUser.Name = "ConcurrentIncognito";
                targetConcurrentUser = ctx.Merge(targetConcurrentUser);
                ctx.Commit();

                Assert.Multiple(() =>
                {
                    Assert.That(detachedUser.Name, Is.Not.EqualTo(targetConcurrentUser.Name));
                    Assert.That(detachedUser.Timestamp, Is.Not.EqualTo(targetConcurrentUser.Timestamp));
                });
            }

            if (!db.TryOpenUnitOfWork(out ctx))
            {
                Assert.Fail();
                return;
            }

            using (ctx)
            {
                detachedUser = ctx.Merge(users[0]);
                var targetUser = ctx.Query<User>().Single(u => users[0].Id == u.Id);
                Assert.Multiple(() =>
                {
                    Assert.That(detachedUser.Name, Is.EqualTo(targetUser.Name));
                    Assert.That(detachedUser.Timestamp, Is.EqualTo(targetUser.Timestamp));
                });
            }
        }
        [Test]
        public void Basic_VersionIncrement_Merge_Test()
        {
            (var users, _, _) = DomainsHelper.GenerateRelativeDomainData();

            var db = _serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
            if (!db.TryOpenUnitOfWork(out var ctx))
            {
                Assert.Fail();
                return;
            }
            using (ctx)
            {
                users = (List<User>)ctx.MergeMany(users);
                ctx.Commit();
            }

            var detachedUser = users[0];
            detachedUser.Name = "Incognito";
            Thread.Sleep(1500);
            if (!db.TryOpenUnitOfWork(out ctx))
            {
                Assert.Fail();
                return;
            }

            using (ctx)
            {
                var targetConcurrentUser = ctx.Query<User>()
                                              .Single(u => users[0].Id == u.Id);
                targetConcurrentUser.Name = "ConcurrentIncognito";
                targetConcurrentUser = ctx.Merge(targetConcurrentUser);
                ctx.Commit();

                Assert.Multiple(() =>
                {
                    Assert.That(detachedUser.Name, Is.Not.EqualTo(targetConcurrentUser.Name));
                    Assert.That(detachedUser.Timestamp, Is.Not.EqualTo(targetConcurrentUser.Timestamp));
                });
            }

            if (!db.TryOpenUnitOfWork(out ctx))
            {
                Assert.Fail();
                return;
            }

            using (ctx)
            {
                var targetUser = ctx.Query<User>().Single(u => users[0].Id == u.Id);
                Assert.Multiple(() =>
                {
                    Assert.That(detachedUser.Name, Is.Not.EqualTo(targetUser.Name));
                    Assert.That(detachedUser.Timestamp, Is.Not.EqualTo(targetUser.Timestamp));
                });
                Assert.Throws<StaleObjectStateException>(() => detachedUser = ctx.Merge(detachedUser));
            }
        }
        [TestCase(5)]
        public async Task Concurrency_AccessTest(int concurrencyLimit)
        {
            var tasks = Enumerable
                .Range(1, concurrencyLimit)
                .Select(i =>
                    Task.Run(() =>
                    {
                        _scopeExecutor.Run(sp =>
                        {
                            var ctx = sp.GetRequiredService<IUnitOfWork>();

                            ctx.Merge(new User { Name = $"Incognito{i}" });
                        });
                    })
                );

            await Task.WhenAll(tasks);

            var db = _serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
            if (!db.TryOpenUnitOfWork(out var ctx))
            {
                Assert.Fail();
                return;
            }

            using (ctx)
            {
                Assert.That(ctx.Query<User>().ToList(), Has.Count.EqualTo(5));
            }
        }
        [TestCase(5)]
        public async Task Concurrency_OptimisticLock_Test(int concurrencyLimit)
        {
            var (users, _, _) = DomainsHelper.GenerateRelativeDomainData();

            await _scopeExecutor.RunAsync(async sp =>
            {
                var ctx = sp.GetRequiredService<IUnitOfWork>();

                await ctx.MergeManyAsync(users);
            });

            var targetId = users[^1].Id;
            var s = DateTime.Now;
            var tasks = Enumerable
                .Range(1, concurrencyLimit)
                .Select(i =>
                    Task.Run(() =>
                    {
                        var success = true;

                        try
                        {
                            _scopeExecutor.Run(sp =>
                            {
                                Thread.Sleep(new Random().Next(1, concurrencyLimit) * 100);
                                TestContext.WriteLine($"{DateTime.Now.Subtract(s).TotalMilliseconds:f0} ms\tuser:{i}\tstart");
                                
                                var ctx = sp.GetRequiredService<IUnitOfWork>();

                                var toEdit =  ctx.Query<User>().Single(u => u.Id == targetId);

                                Thread.Sleep(new Random().Next(concurrencyLimit, concurrencyLimit ^ 2) * 1000);
                                TestContext.WriteLine($"{DateTime.Now.Subtract(s).TotalMilliseconds:f0} ms\tuser:{i}\tflush");
                                toEdit.Name = $"HELLO WORLD FROM user{i}";

                                ctx.Merge(toEdit);
                            });
                        }
                        catch (StaleObjectStateException)
                        {
                            success = false;
                        }

                        TestContext.WriteLine($"{DateTime.Now.Subtract(s).TotalMilliseconds:f0} ms\tuser:{i}\tend succeeded:{success}");
                        return success;
                    })
                );

            var results = await Task.WhenAll(tasks);
            Assert.That(results.Count(succeeded => succeeded), Is.EqualTo(1));
        }
    }
}
