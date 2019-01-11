using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngularASPNETCore2WebApiAuth.Api.ViewModels
{
    public class CredentialsViewModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        [FromForm(Name = "refresh_token")]
        public string RefreshToken { get; set; }

        [FromForm(Name = "grant_type")]
        public string GrantType { get; set; }

        [FromForm(Name = "client_id")]
        public string ClientId { get; set; }
    }
}
