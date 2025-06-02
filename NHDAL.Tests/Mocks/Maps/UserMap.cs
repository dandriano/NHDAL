using NHDAL.Tests.Mocks.Entities;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHDAL.Tests.Mocks.Maps
{
    internal partial class UserMap : ClassMapping<User>
    {
        public UserMap()
        {
            Table("\"users\"");
            Schema("\"public\"");
            Lazy(true);
            Id(x => x.Id, map => { map.Column("\"id\""); map.Generator(Generators.Assigned); });
            Property(x => x.Name, map => map.Column("\"name\""));

            Set(x => x.Posts, colmap => { colmap.Key(x => x.Column("\"author_id\"")); colmap.Inverse(true); colmap.Cascade(Cascade.All | Cascade.DeleteOrphans); }, map => { map.OneToMany(); });
            Set(x => x.Comments, colmap => { colmap.Key(x => x.Column("\"author_id\"")); colmap.Inverse(true); colmap.Cascade(Cascade.All | Cascade.DeleteOrphans); }, map => { map.OneToMany(); });
        }
    }
}
