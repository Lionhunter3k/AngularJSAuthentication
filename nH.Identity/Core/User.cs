using System;
using System.Collections.Generic;
using System.Text;

namespace nH.Identity.Core
{
    public class User : BaseEntity<string>
    {
        public virtual string DisplayName { get; set; }

        public virtual string Email { get; set; }

        public virtual bool EmailConfirmed { get; set; }

        public virtual DateTime? EmailConfirmedOnUtc { get; set; }

        public virtual string PhoneNumber { get; set; }

        public virtual bool PhoneNumberConfirmed { get; set; }

        public virtual DateTime? PhoneNumberConfirmedOnUtc { get; set; }

        public virtual string PasswordHash { get; set; }

        public virtual string SecurityStamp { get; set; }

        public virtual bool Deleted { get; set; }

        public virtual int AccessFailedCount { get; set; }

        public virtual bool LockoutEnabled { get; set; }

        public virtual DateTime? LockoutEndUtc { get; set; }

        public virtual bool TwoFactorEnabled { get; set; }

        public virtual ISet<UserLogin> UserLogins { get; set; } = new HashSet<UserLogin>();

        public virtual ISet<UserClaim> UserClaims { get; set; } = new HashSet<UserClaim>();

        public virtual ISet<UserToken> UserTokens { get; set; } = new HashSet<UserToken>();

        public virtual ISet<Role> Roles { get; set; } = new HashSet<Role>();
    }
}
