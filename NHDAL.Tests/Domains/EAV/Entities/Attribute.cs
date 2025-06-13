using NHDAL.Interfaces;
using System;

namespace NHDAL.Tests.Domains.EAV.Entities
{
    public class Attribute : IEntity<Guid>
    {
        public virtual Guid Id { get; init; } = Guid.NewGuid();
        public virtual DateTime Timestamp { get; protected set; }
        public virtual Entity EntityType { get; set; } = null!;
        public virtual string Name { get; set; } = string.Empty;
        public virtual string ValueType { get; set; } = string.Empty;
        public virtual string Description { get; set; } = string.Empty;
        public virtual string DisplayName { get; set; } = string.Empty;
    }
}