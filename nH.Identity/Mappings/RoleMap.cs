using nH.Identity.Core;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Text;

namespace nH.Identity.Mappings
{
    public class RoleMap : ClassMapping<Role>
    {
        public RoleMap()
        {
            Id(x => x.Id, map => { map.Generator(Generators.SequenceHiLo); });
            Property(x => x.Name, map => { map.NotNullable(true); map.Length(200); map.Unique(true); });

            Set(x => x.RoleClaims, colmap => { colmap.Key(x => x.Column("RoleId")); colmap.Inverse(true); colmap.Cascade(Cascade.All | Cascade.DeleteOrphans); }, map => { map.OneToMany(); });
        }
    }
}
