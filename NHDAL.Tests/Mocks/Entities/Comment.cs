using System;

namespace NHDAL.Tests.Mocks.Entities
{
    internal class Comment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public User Author { get; set; } = User.Nobody;
        public Post Post { get; set; } = Post.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
