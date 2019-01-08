using nH.Identity.Core;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Text;

namespace nH.Identity.Mappings
{
    public class UserLoginMap : ClassMapping<UserLogin>
    {
        public UserLoginMap()
        {
            Id(x => x.Id, map => { map.Generator(Generators.SequenceHiLo); });
            Property(x => x.LoginProvider, map => { map.NotNullable(true); map.Length(200); });
            Property(x => x.ProviderDisplayName, map => { map.NotNullable(true); map.Length(200); });
            Property(x => x.ProviderKey, map => { map.NotNullable(true); map.Length(200); });
            ManyToOne(x => x.User, map =>
            {
                map.Column("UserId");
                map.NotNullable(true);
            });
        }
    }
}
