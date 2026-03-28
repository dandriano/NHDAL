using NHDAL.Interfaces;
using NHibernate.Envers.Configuration.Attributes;
using System;

namespace NHDAL.Tests.Domains.EAV.Entities
{
    [Audited]
    [AuditTable("attributes_aud")]
    public class Attribute : IEntity<Guid>
    {
        public virtual Guid Id { get; init; } = Guid.Empty;
        public virtual DateTime Timestamp { get; protected set; }
        public virtual Entity EntityType { get; set; } = null!;
        public virtual string Name { get; set; } = string.Empty;
        public virtual string ValueType { get; set; } = string.Empty;
        public virtual string Description { get; set; } = string.Empty;
        public virtual string DisplayName { get; set; } = string.Empty;
    }
}