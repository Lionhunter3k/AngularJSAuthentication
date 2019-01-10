using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Text;
using TokenApi.Entities;

namespace TokenApi.Entities.Mappings
{
    public class RefreshTokenMap : ClassMapping<RefreshToken>
    {
        public RefreshTokenMap()
        {
            Id(x => x.Id, map => { map.Generator(Generators.SequenceHiLo); });
            Property(x => x.Token, map => { map.NotNullable(true); map.Length(1000); });
            Property(x => x.Type, map => { map.NotNullable(true); map.Length(1000); });
            Property(x => x.IssuedUtc, map => map.NotNullable(true));
            Property(x => x.ExpiresUtc, map => map.NotNullable(true));
            ManyToOne(x => x.User, map =>
            {
                map.Column("UserId");
                map.NotNullable(true);
            });
        }
    }
}
