using NHDAL.Tests.Mocks.Entities;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHDAL.Tests.Mocks.Maps
{
    internal partial class PostMap : ClassMapping<Post>
    {
        public PostMap()
        {
            Table("\"posts\"");
            Schema("\"public\"");
            Lazy(true);
            Id(x => x.Id, map => { map.Column("\"id\""); map.Generator(Generators.Assigned); });
            Property(x => x.Text, map => map.Column("\"text\""));
            ManyToOne(x => x.Author, map => { map.Column("\"author_id\""); map.Cascade(Cascade.None); });
            Set(x => x.Comments, colmap => { colmap.Key(x => x.Column("\"user_id\"")); colmap.Inverse(true); colmap.Cascade(Cascade.All | Cascade.DeleteOrphans); }, map => { map.OneToMany(); });
        }
    }
}
