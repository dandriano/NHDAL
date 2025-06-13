using NHDAL.Interfaces;
using System;
using System.Collections.Generic;

namespace NHDAL.Tests.Mocks.Entities
{
    internal class Entity : IEntity<Guid>
    {
        public virtual Guid Id { get; init; } = Guid.NewGuid();
        public virtual DateTime Timestamp { get; protected set; }
        public virtual string Description { get; set; } = string.Empty;
        public virtual string Name { get; set; } = string.Empty;
        public virtual ISet<EntityRecord> EntityRecords { get; set; } = new HashSet<EntityRecord>();
    }
}