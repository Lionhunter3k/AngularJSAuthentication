using nH.Identity.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace TokenApi.Entities
{
    public class RefreshToken : BaseEntity<long>
    {
        public virtual DateTime IssuedUtc { get; set; }

        public virtual DateTime ExpiresUtc { get; set; }

        public virtual string Token { get; set; }

        public virtual User User { get; set; }

        public virtual string Type { get; set; }
    }
}
