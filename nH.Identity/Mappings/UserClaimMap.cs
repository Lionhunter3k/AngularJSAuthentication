using nH.Identity.Core;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Text;

namespace nH.Identity.Mappings
{
    public class UserClaimMap : ClassMapping<UserClaim>
    {
        public UserClaimMap()
        {
            Id(x => x.Id, map => { map.Generator(Generators.HighLow); });
            Property(x => x.ClaimType, map => { map.NotNullable(true); map.Length(200); });
            Property(x => x.ClaimValue, map => { map.NotNullable(true); map.Length(200); });
            ManyToOne(x => x.User, map =>
            {
                map.Column("UserId");
                map.NotNullable(true);
            });
        }
    }
}
