using System;
using System.Collections.Generic;

namespace NHDAL.Tests.Mocks.Entities
{
    internal class Post
    {
        public static Post Empty { get; set; } = new Post() { Id = Guid.Empty };
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        public virtual User Author { get; set; } = User.Nobody;
        public virtual string Text { get; set; } = string.Empty;
        public virtual ISet<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}
