using nH.Identity.Core;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Text;

namespace nH.Identity.Mappings
{
    public class UserMap : ClassMapping<User>
    {
        public UserMap()
        {
            Id(x => x.Id, map => { map.Generator(Generators.Assigned); map.Length(50); });
            Property(x => x.PasswordHash, map => { map.NotNullable(true); map.Length(200); });
            Property(x => x.SecurityStamp, map => { map.NotNullable(true); map.Length(50); });
            Property(x => x.DisplayName, map => { map.Length(50); });
            Property(x => x.Email, map => { map.NotNullable(true); map.Length(50); });
            Property(x => x.EmailConfirmed);
            Property(x => x.EmailConfirmedOnUtc);
            Property(x => x.PhoneNumber, map => { map.NotNullable(true); map.Length(20); });
            Property(x => x.PhoneNumberConfirmed);
            Property(x => x.PhoneNumberConfirmedOnUtc);
            Property(x => x.Deleted);
            Property(x => x.AccessFailedCount);
            Property(x => x.TwoFactorEnabled);
            Property(x => x.LockoutEnabled);
            Property(x => x.LockoutEndUtc);
            Set(x => x.UserClaims, colmap => { colmap.Key(x => x.Column("UserId")); colmap.Inverse(true); colmap.Cascade(Cascade.All | Cascade.DeleteOrphans); }, map => { map.OneToMany(); });
            Set(x => x.UserLogins, colmap => { colmap.Key(x => x.Column("UserId")); colmap.Inverse(true); colmap.Cascade(Cascade.All | Cascade.DeleteOrphans); }, map => { map.OneToMany(); });
            Set(x => x.UserTokens, colmap => { colmap.Key(x => x.Column("UserId")); colmap.Inverse(true); colmap.Cascade(Cascade.All | Cascade.DeleteOrphans); }, map => { map.OneToMany(); });

            Set(x => x.Roles, collectionMapping =>
            {
                collectionMapping.Table("UserRole");
                collectionMapping.Cascade(Cascade.None);
                collectionMapping.Key(k => k.Column("UserId"));
                collectionMapping.Inverse(false);
            }, map => map.ManyToMany(p => p.Column("RoleId")));
        }
    }
}
