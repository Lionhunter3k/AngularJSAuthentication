using nH.Identity.Core;
using System.Collections.Generic;

namespace ASOS.Identity.Api.Entities
{
    public enum ApplicationType
    {
        Public,
        Confidential
    }

    public class ClientApplication : BaseEntity<long>
    {
        public virtual string Name { get; set; }

        public virtual ApplicationType Type { get; set; }

        public virtual string Secret { get; set; }

        public virtual List<string> AllowedGrants { get; set; }
    }
}
