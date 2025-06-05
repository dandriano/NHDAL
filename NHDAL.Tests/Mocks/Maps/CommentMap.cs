using System;
using NHDAL.Tests.Mocks.Entities;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;

namespace NHDAL.Tests.Mocks.Maps
{
    internal class CommentMap : ClassMapping<Comment>
    {
        public CommentMap()
        {
            Table("\"comments\"");
            Schema("\"public\"");
            Lazy(true);
            DynamicUpdate(true);

            Id(x => x.Id, map => { map.Column("\"id\""); map.Generator(Generators.Assigned); });
            Version(x => x.Timestamp, map =>
            {
                map.Column(c =>
                    {
                        c.Name("\"version\"");
                        c.NotNullable(true);
                        c.Default("CURRENT_TIMESTAMP");
                    });
                map.Generated(VersionGeneration.Always);
                map.Type(new DateTimeType());
                map.UnsavedValue(DateTime.MinValue);
            });
            Property(x => x.Text, map => map.Column("\"text\""));
            ManyToOne(x => x.Author, map => { map.Column("\"author_id\""); map.Cascade(Cascade.None); });
        }
    }
}
