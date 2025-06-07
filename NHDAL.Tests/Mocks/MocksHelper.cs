using NHDAL.Tests.Mocks.Entities;
using System.Collections.Generic;
using System.Linq;

namespace NHDAL.Tests.Mocks
{
    internal static class MocksHelper
    {
        public static (List<User> users, List<Post> posts, List<Comment> comments) GenerateMockDomainData()
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

            return (users, posts, comments);
        }
    }
}