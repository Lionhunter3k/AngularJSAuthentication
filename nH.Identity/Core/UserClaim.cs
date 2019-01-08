using System;
using System.Collections.Generic;
using System.Text;

namespace nH.Identity.Core
{
    public class UserClaim : BaseEntity<long>
    {
        public virtual User User { get; set; }
        /// <summary>
        ///     Gets or sets the claim type for this claim.
        /// </summary>
        public virtual string ClaimType { get; set; }

        /// <summary>
        ///     Gets or sets the claim value for this claim.
        /// </summary>
        public virtual string ClaimValue { get; set; }
    }
}
