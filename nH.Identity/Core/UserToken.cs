using System;
using System.Collections.Generic;
using System.Text;

namespace nH.Identity.Core
{
    public class UserToken : BaseEntity<long>
    {
        public virtual User User { get; set; }
        /// <summary>
		///     Gets or sets the LoginProvider this token is from.
		/// </summary>
		public virtual string LoginProvider { get; set; }

        /// <summary>
        ///     Gets or sets the name of the token.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        ///     Gets or sets the token value.
        /// </summary>
        public virtual string Value { get; set; }
    }
}
