using nH.Identity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAuthTutorial.Entities
{
    public class OAuthClient
    {
        public virtual string ClientId { get; set; }

        /* Each App needs a Client Secret, but it is assigned at creation */
        public virtual string ClientSecret { get; set; }

        /* Each App Needs an Owner, which will be assigned at creation. This is also a Foreign Key to the Users table. */
        public virtual User Owner { get; set; }

        /* This field, combined with the RedirectURI.OAuthClient field, lets EntityFramework know that this is a (1 : Many) mapping */
        public virtual IList<string> RedirectURIs { get; set; } = new List<string>();

        /*  Like above, this notifies EntityFramework of another (1 : Many) mapping */
        public virtual ISet<Token> UserApplicationTokens { get; set; } = new HashSet<Token>();

        /* Each App needs a Name, which is submitted by the user at Creation time */
        public virtual string ClientName { get; set; }

        /* Each App needs a Description, which is submitted by the user at Edit time */
        public virtual string ClientDescription { get; set; }
    }
}
