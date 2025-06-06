using Microsoft.Extensions.DependencyInjection;
using NHDAL.Tests.Mocks.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NHDAL.Tests
{
    [TestFixture]
    internal class ConcurrencyTests : RegistrarBase
    {
        private UnitOfWorkFactory _db;

        [SetUp]
        public void SetUp()
        {
            _db = _serviceProvider.GetRequiredService<UnitOfWorkFactory>();
            _db.BuildSchema();
        }
        [TestCase(5)]
        public async Task OptimisticConcurrencyTest(int concurrencyLimit)
        {
            var users = new List<User>()
            {
                new User { Name = "AlrigthAlrightAlright" },
                new User { Name = "SillySandler" },
                new User { Name = "PerspectiveDiCaprio" },
            };
            var targetId = users[^1].Id;
            using (var ctx = _db.OpenUnitOfWork())
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
                        // is only working within the bounds of the ISession 
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
                        catch
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
