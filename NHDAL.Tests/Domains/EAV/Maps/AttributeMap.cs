using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using System;

namespace NHDAL.Tests.Domains.EAV.Maps
{
    public class AttributeMap : ClassMapping<Entities.Attribute>
    {
        public AttributeMap()
        {
            Table("\"attributes\"");
            Schema("\"public\"");

            Lazy(true);
            OptimisticLock(OptimisticLockMode.Version);
            DynamicUpdate(true);

            Id(x => x.Id, map => { map.Column("\"id\""); map.Generator(Generators.Assigned); });
            Version(x => x.Timestamp, map =>
            {
                map.Column("\"version\"");
                map.Generated(VersionGeneration.Never);
                map.Type(new DateTimeType());
                map.UnsavedValue(DateTime.MinValue);
            });

            Property(x => x.Name, map => map.Column("\"name\""));
            Property(x => x.Description, map => map.Column("\"description\""));
            Property(x => x.DisplayName, map => map.Column("\"display_name\""));
            Property(x => x.ValueType, map => map.Column("\"value_type\""));
            ManyToOne(x => x.EntityType, map => { map.Column("\"entity_id\""); map.Cascade(Cascade.None); });
        }
    }
}