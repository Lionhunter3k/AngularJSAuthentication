using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngularASPNETCore2WebApiAuth.Api.Entities.Mappings
{
    public class RefreshTokenMap : ClassMapping<RefreshToken>
    {
        public RefreshTokenMap()
        {
            Table("RefreshToken2");
            Id(x => x.Id, map => { map.Generator(Generators.GuidComb); });
            Property(x => x.Token, map => { map.NotNullable(true); map.Length(1000); });
            Property(x => x.ClientId, map => { map.NotNullable(true); map.Length(1000); });
            Property(x => x.RemoteIpAddress, map => map.NotNullable(true));
            Property(x => x.ExpiresUtc, map => map.NotNullable(true));
            ManyToOne(x => x.User, map =>
            {
                map.Column("UserId");
                map.NotNullable(true);
            });
        }
    }
}
