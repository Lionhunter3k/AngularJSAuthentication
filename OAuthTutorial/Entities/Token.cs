using nH.Identity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAuthTutorial.Entities
{
    public class Token
    {

        public virtual int TokenId { get; set; }

        /* How this token was created: 'token', 'authorization_code', 'client_credentials', 'refresh' */
        public virtual string GrantType { get; set; }

        /* Access, Refresh */
        public virtual string TokenType { get; set; }

        /* The raw value of a token. */
        public virtual string Value { get; set; }

        /* Entity Framework Foreign Key Anchors for OAuth Clients */
        public virtual OAuthClient Client { get; set; }

        public virtual User User { get; set; }
    }
}
