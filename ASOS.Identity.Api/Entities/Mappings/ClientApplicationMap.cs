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
            Id(x => x.Id, map => { map.Generator(Generators.Assigned); });
            Property(x => x.Secret, map => { map.Length(200); });
            Property(x => x.Type);
            List(x => x.AllowedGrants, c =>
            {
                c.Index(idx =>
                {
                    idx.Base(1);
                    idx.Column("GrantIndex");
                });
                c.Cascade(Cascade.All | Cascade.DeleteOrphans);
            }, r => r.Element(m =>
            {
                m.Column("GrantType");
                m.Length(100);
                m.NotNullable(true);
            }));
            List(x => x.AllowedRedirectUris, c =>
            {
                c.Index(idx =>
                {
                    idx.Base(1);
                    idx.Column("RedirectUriIndex");
                });
                c.Cascade(Cascade.All | Cascade.DeleteOrphans);
            }, r => r.Element(m =>
            {
                m.Column("RedirectUri");
                m.Length(100);
                m.NotNullable(true);
            }));
        }
    }
}
