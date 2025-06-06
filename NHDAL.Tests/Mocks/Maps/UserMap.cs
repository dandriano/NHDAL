using NHDAL.Tests.Mocks.Entities;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using System;

namespace NHDAL.Tests.Mocks.Maps
{
    internal class UserMap : ClassMapping<User>
    {
        public UserMap()
        {
            Table("\"users\"");
            Schema("\"public\"");

            Lazy(true);
            DynamicUpdate(true);
            OptimisticLock(OptimisticLockMode.Version);

            Id(x => x.Id, map => { map.Column("\"id\""); map.Generator(Generators.Assigned); });
            Version(x => x.Timestamp, map =>
            {
                map.Column("\"version\"");
                map.Generated(VersionGeneration.Never);
                map.Type(new DateTimeType());
                map.UnsavedValue(DateTime.MinValue);
            });

            Property(x => x.Name, map => map.Column("\"name\""));
            Set(x => x.Posts, colmap => { colmap.Key(x => x.Column("\"author_id\"")); colmap.Inverse(true); colmap.Cascade(Cascade.All | Cascade.DeleteOrphans); }, map => { map.OneToMany(); });
            Set(x => x.Comments, colmap => { colmap.Key(x => x.Column("\"author_id\"")); colmap.Inverse(true); colmap.Cascade(Cascade.All | Cascade.DeleteOrphans); }, map => { map.OneToMany(); });
        }
    }
}
