using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAuthTutorial.Entities.Mappings
{
    public class OAuthClientMap : ClassMapping<OAuthClient>
    {
        public OAuthClientMap()
        {
            Id(x => x.ClientId, map => { map.Generator(Generators.Assigned); });
            Property(x => x.ClientSecret, map => { map.NotNullable(true); map.Length(1000); });
            Property(x => x.ClientName, map => { map.NotNullable(true); map.Length(100); });
            Property(x => x.ClientDescription, map => { map.NotNullable(true); map.Length(300); });
            ManyToOne(x => x.Owner, map =>
            {
                map.Column("Id");
                map.NotNullable(true);
            });
            Bag(x => x.RedirectURIs, c =>
            {
                c.Key(k => k.Column("ClientId"));
                c.Cascade(Cascade.All | Cascade.DeleteOrphans);
            }, r => r.Element(m =>
            {
                m.Column("URI");
                m.Length(100);
                m.NotNullable(true);
            }));
            Set(x => x.UserApplicationTokens, colmap => { colmap.Key(x => x.Column("ClientId")); colmap.Inverse(true); colmap.Cascade(Cascade.All | Cascade.DeleteOrphans); }, map => { map.OneToMany(); });
        }
    }
}
