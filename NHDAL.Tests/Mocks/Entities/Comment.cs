using System;
using NHDAL.Interfaces;

namespace NHDAL.Tests.Mocks.Entities
{
    internal class Comment : IEntity<Guid>
    {
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        public virtual User Author { get; set; } = User.Nobody;
        public virtual Post Post { get; set; } = Post.Empty;
        public virtual string Text { get; set; } = string.Empty;
        public virtual DateTime Timestamp { get; set; }
    }
}
