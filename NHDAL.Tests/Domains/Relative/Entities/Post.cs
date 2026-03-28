using NHDAL.Interfaces;
using System;
using System.Collections.Generic;

namespace NHDAL.Tests.Domains.Relative.Entities
{
    public class Post : IEntity<Guid>
    {
        public virtual Guid Id { get; init; } = Guid.Empty;
        public virtual DateTime Timestamp { get; protected set; }
        public virtual User Author { get; set; } = null!;
        public virtual string Text { get; set; } = string.Empty;
        public virtual ISet<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}
