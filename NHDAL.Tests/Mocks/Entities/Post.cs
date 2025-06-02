using System;
using System.Collections.Generic;

namespace NHDAL.Tests.Mocks.Entities
{
    internal class Post
    {
        public static Post Empty { get; set; } = new Post() { Id = Guid.Empty };
        public Guid Id { get; set; } = Guid.NewGuid();
        public User Author { get; set; } = User.Nobody;
        public string Text { get; set; } = string.Empty;
        public ISet<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}
