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
    }
}
