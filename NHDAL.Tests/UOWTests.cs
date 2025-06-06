using Microsoft.Extensions.DependencyInjection;
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
            using var ctx = _db.OpenUnitOfWork();
            var users = ctx.Query<User>().ToList();

            Assert.That(users, Is.Empty);
        }
        [Test]
        public void Basic_InsertTest()
        {
            var users = new List<User>()
            {
                new User { Name = "AlrigthAlrightAlright" },
                new User { Name = "SillySandler" },
                new User { Name = "PerspectiveDiCaprio" },
            };

            var userByName = users.ToDictionary(u => u.Name);

            var posts = new List<Post>
            {
                new Post
                {
                    Author = userByName["PerspectiveDiCaprio"],
                    Text = "Hello World!",
                },
                new Post
                {
                    Author = userByName["AlrigthAlrightAlright"],
                    Text = "Is anyone here?",
                },
                new Post
                {
                    Author = userByName["SillySandler"],
                    Text = "My blog is here!",
                }
            };

            // add posts to users
            foreach (var post in posts)
            {
                post.Author.Posts.Add(post);
            }

            var comments = new List<Comment>
            {
                new Comment
                {
                    Author = userByName["SillySandler"],
                    Post = posts[0],
                    Text = "Nice post!",
                },
                new Comment
                {
                    Author = userByName["AlrigthAlrightAlright"],
                    Post = posts[0],
                    Text = "Thanks for sharing.",
                },
                new Comment
                {
                    Author = userByName["PerspectiveDiCaprio"],
                    Post = posts[1],
                    Text = "Absolutely agree!",
                },
                new Comment
                {
                    Author = userByName["PerspectiveDiCaprio"],
                    Post = posts[2],
                    Text = "Good morning to you too!",
                }
            };

            // Add comments to posts and users
            foreach (var comment in comments)
            {
                comment.Post.Comments.Add(comment);
                comment.Author.Comments.Add(comment);
            }

            using (var ctx1 = _db.OpenUnitOfWork())
            {
                ctx1.MergeMany(users);
                ctx1.Commit();
            }

            using var ctx2 = _db.OpenUnitOfWork();
            Assert.That(ctx2.Query<User>().ToList(), Has.Count.EqualTo(users.Count));
            Assert.That(ctx2.Query<Post>().ToList(), Has.Count.EqualTo(posts.Count));
            Assert.That(ctx2.Query<Comment>().ToList(), Has.Count.EqualTo(comments.Count));
        }

        [TestCase(5)]
        public async Task OptimisticConcurrency_WithinBoundariesOfUOW_Test(int concurrencyLimit)
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
