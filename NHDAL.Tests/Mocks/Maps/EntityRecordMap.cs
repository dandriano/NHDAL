using NHDAL.Tests.Misc;
using NHDAL.Tests.Mocks.Entities;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using System;
using System.Collections.Generic;

namespace NHDAL.Tests.Mocks.Maps
{
    internal class EntityRecrodMap : ClassMapping<EntityRecord>
    {
        public EntityRecrodMap()
        {
            Table("\"entity_records\"");
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

            Property(x => x.ProjectId, map => { map.Column("\"project_id\""); map.NotNullable(true); });
            Property(x => x.AttributeMap, map =>
            {
                map.Column(c =>
                {
                    c.Name("\"attribute_map\"");
                    c.SqlType("jsonb");
                });
                map.Type(new CustomColumnType(typeof(JsonType<HashSet<AttributeRecord>>), null));
                map.NotNullable(true);
            });
            Property(x => x.RelationMap, map =>
            {
                map.Column(c =>
                {
                    c.Name("\"relation_map\"");
                    c.SqlType("jsonb");
                });
                map.Type(new CustomColumnType(typeof(JsonType<HashSet<EntityRecordRelation>>), null));
                map.NotNullable(true);
            });
            ManyToOne(x => x.EntityType, map => { map.Column("\"entity_id\""); map.Cascade(Cascade.None); });
        }
    }
}