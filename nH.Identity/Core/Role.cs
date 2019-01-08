using System;
using System.Collections.Generic;
using System.Text;

namespace nH.Identity.Core
{
    public class Role : BaseEntity<long>
    {
        public virtual string Name { get; set; }

        public virtual ISet<RoleClaim> RoleClaims { get; set; } = new HashSet<RoleClaim>();
    }
}
