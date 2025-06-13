using NHDAL.Interfaces;
using System;
using System.Collections.Generic;

namespace NHDAL.Tests.Mocks.Entities
{
    internal class EntityRecord : IEntity<Guid>
    {
        public virtual Guid Id { get; init; } = Guid.NewGuid();
        public virtual DateTime Timestamp { get; protected set; }
        public virtual Guid ProjectId { get; set; }
        public virtual Entity? EntityType { get; set; }
        public virtual ISet<AttributeRecord> AttributeMap { get; set; } = new HashSet<AttributeRecord>();
        public virtual ISet<EntityRecordRelation> RelationMap { get; set; } = new HashSet<EntityRecordRelation>();
    }

    internal class AttributeRecord
    {
        public virtual Guid Id { get; init; } = Guid.NewGuid();
        public virtual Attribute? Attribute { get; set; }
        public virtual string Value { get; set; } = string.Empty;
        public virtual string ValueType { get; set; } = string.Empty;
    }

    internal class EntityRecordRelation
    {
        public virtual Guid Id { get; init; } = Guid.NewGuid();
        public virtual EntityRecord? RelatedRecord { get; protected set; }
        public virtual int RelationType { get; protected set; }
    }
}