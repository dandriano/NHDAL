using NHDAL.Interfaces;
using NHibernate.Envers.Configuration.Attributes;
using System;
using System.Collections.Generic;

namespace NHDAL.Tests.Domains.EAV.Entities
{
    [Audited]
    [AuditTable("entity_records_aud")]
    public class EntityRecord : IEntity<Guid>
    {
        public virtual Guid Id { get; init; } = Guid.Empty;
        public virtual DateTime Timestamp { get; protected set; }
        public virtual Guid ProjectId { get; set; }
        public virtual Entity EntityType { get; set; } = null!;
        public virtual ISet<AttributeRecord> AttributeMap { get; set; } = new HashSet<AttributeRecord>();
        public virtual ISet<EntityRecordRelation> RelationMap { get; set; } = new HashSet<EntityRecordRelation>();
    }

    public class AttributeRecord
    {
        public virtual Guid Id { get; init; } = Guid.NewGuid();
        public virtual Guid AttributeId { get; set; } = Guid.Empty;
        public virtual string Value { get; set; } = string.Empty;
    }

    public class EntityRecordRelation
    {
        public virtual Guid Id { get; init; } = Guid.NewGuid();
        public virtual EntityRecord? RelatedRecord { get; protected set; }
        public virtual int RelationType { get; protected set; }
    }
}