using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAuthTutorial.Entities.Mappings
{
    public class TokenMap : ClassMapping<Token>
    {
        public TokenMap()
        {
            Id(x => x.TokenId, map => { map.Generator(Generators.HighLow); });
            Property(x => x.GrantType);
            Property(x => x.TokenType);
            Property(x => x.Value);
            ManyToOne(x => x.Client, map =>
            {
                map.Column("ClientId");
                map.NotNullable(true);
            });
            ManyToOne(x => x.User, map =>
            {
                map.Column("UserId");
                map.NotNullable(true);
            });
        }
    }
}
