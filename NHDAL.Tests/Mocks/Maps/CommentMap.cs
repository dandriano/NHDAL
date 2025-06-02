using NHDAL.Tests.Mocks.Entities;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHDAL.Tests.Mocks.Maps
{
    internal class CommentMap : ClassMapping<Comment>
    {
        public CommentMap()
        {
            Table("\"comments\"");
            Schema("\"public\"");
            Lazy(true);
            Id(x => x.Id, map => { map.Column("\"id\""); map.Generator(Generators.Assigned); });
            Property(x => x.Text, map => map.Column("\"text\""));
            ManyToOne(x => x.Author, map => { map.Column("\"author_id\""); map.Cascade(Cascade.None); });
        }
    }
}
