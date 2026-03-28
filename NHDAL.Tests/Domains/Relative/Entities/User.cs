using NHDAL.Interfaces;
using System;
using System.Collections.Generic;

namespace NHDAL.Tests.Domains.Relative.Entities
{
    public class User : IEntity<Guid>
    {
        public virtual Guid Id { get; init; } = Guid.Empty;
        public virtual DateTime Timestamp { get; protected set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual ISet<Post> Posts { get; set; } = new HashSet<Post>();
        public virtual ISet<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}
