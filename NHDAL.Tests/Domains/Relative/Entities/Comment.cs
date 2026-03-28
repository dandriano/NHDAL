using NHDAL.Interfaces;
using System;

namespace NHDAL.Tests.Domains.Relative.Entities
{
    public class Comment : IEntity<Guid>
    {
        public virtual Guid Id { get; init; } = Guid.Empty;
        public virtual DateTime Timestamp { get; protected set; }
        public virtual User Author { get; set; } = null!;
        public virtual Post Post { get; set; } = null!;
        public virtual string Text { get; set; } = string.Empty;
    }
}
