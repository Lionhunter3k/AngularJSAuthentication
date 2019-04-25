using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAuthTutorial.Models.AuthorizeViewModels
{
    public class AuthorizeViewModel
    {

        public string ClientName { get; set; }

        public string ClientId { get; set; }

        public string ClientDescription { get; set; }

        public string ResponseType { get; set; }

        public string RedirectUri { get; set; }

        public string[] Scopes { get; set; } = new string[0];

        public string State { get; set; }
    }
}
