using NHDAL.Interfaces;
using System;

namespace NHDAL.Tests.Mocks.Entities
{
    internal class Comment : IEntity<Guid>
    {
        public virtual User Author { get; set; } = User.Nobody;

        public virtual Guid Id { get; protected set; } = Guid.NewGuid();
        public virtual DateTime Timestamp { get; protected set; }
        public virtual Post Post { get; set; } = Post.Empty;
        public virtual string Text { get; set; } = string.Empty;
    }
}
