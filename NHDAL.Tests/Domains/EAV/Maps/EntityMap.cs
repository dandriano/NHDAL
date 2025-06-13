using NHDAL.Tests.Domains.EAV.Entities;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using System;

namespace NHDAL.Tests.Domains.EAV.Maps
{
    public class EntityMap : ClassMapping<Entity>
    {
        public EntityMap()
        {
            Table("\"entities\"");
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

            Property(x => x.Description, map => map.Column("\"description\""));
            Property(x => x.Name, map => map.Column("\"name\""));
            Set(x => x.Attributes, colmap => { colmap.Key(x => x.Column("\"entity_id\"")); colmap.Inverse(true); colmap.Cascade(Cascade.All | Cascade.DeleteOrphans); }, map => { map.OneToMany(); });
            Set(x => x.EntityRecords, colmap => { colmap.Key(x => x.Column("\"entity_id\"")); colmap.Inverse(true); colmap.Cascade(Cascade.All | Cascade.DeleteOrphans); }, map => { map.OneToMany(); });
        }
    }
}