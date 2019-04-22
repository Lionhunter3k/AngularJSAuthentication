using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASOS.Identity.Api.Entities.Mappings
{
    public class ClientApplicationMap : ClassMapping<ClientApplication>
    {
        public ClientApplicationMap()
        {
            Table("`ClientApplication`");
            Id(x => x.Id, map => { map.Generator(Generators.HighLow); });
            Property(x => x.Name, map => { map.NotNullable(true); map.Length(200); map.Unique(true); });
            Property(x => x.Secret, map => { map.NotNullable(true); map.Length(200); });
            Property(x => x.Type);
            List(x => x.AllowedGrants, c =>
            {
                c.Index(idx =>
                {
                    idx.Base(1);
                    idx.Column("GrantIndex");
                });
            }, r => r.Element(m =>
            {
                m.Column("GrantType");
                m.Length(100);
                m.NotNullable(true);
            }));
        }
    }
}
