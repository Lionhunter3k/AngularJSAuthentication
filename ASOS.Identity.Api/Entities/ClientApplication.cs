using nH.Identity.Core;
using System.Collections.Generic;

namespace ASOS.Identity.Api.Entities
{
    public enum ApplicationType
    {
        Public,
        Confidential
    }

    public class ClientApplication : BaseEntity<string>
    {
        public virtual ApplicationType Type { get; set; }

        public virtual string Secret { get; set; }

        public virtual IList<string> AllowedGrants { get; set; }
        public virtual IList<string> AllowedRedirectUris { get; set; }
    }
}
