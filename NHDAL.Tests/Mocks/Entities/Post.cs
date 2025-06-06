using NHDAL.Interfaces;
using System;
using System.Collections.Generic;

namespace NHDAL.Tests.Mocks.Entities
{
    internal class Post : IEntity<Guid>
    {
        public static Post Empty { get; set; } = new Post() { Id = Guid.Empty };

        public virtual Guid Id { get; protected set; } = Guid.NewGuid();
        public virtual DateTime Timestamp { get; protected set; }
        public virtual User Author { get; set; } = User.Nobody;
        public virtual string Text { get; set; } = string.Empty;
        public virtual ISet<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}
