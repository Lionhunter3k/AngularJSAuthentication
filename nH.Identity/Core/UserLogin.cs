using System;
using System.Collections.Generic;
using System.Text;

namespace nH.Identity.Core
{
    public class UserLogin : BaseEntity<long>
    {
        public virtual User User { get; set; }

        /// <summary>
		///     Gets or sets the login provider for the login (e.g. facebook, google)
		/// </summary>
		public virtual string LoginProvider { get; set; }

        /// <summary>
        ///     Gets or sets the unique provider identifier for this login.
        /// </summary>
        public virtual string ProviderKey { get; set; }

        /// <summary>
        ///     Gets or sets the friendly name used in a UI for this login.
        /// </summary>
        public virtual string ProviderDisplayName { get; set; }
    }
}
