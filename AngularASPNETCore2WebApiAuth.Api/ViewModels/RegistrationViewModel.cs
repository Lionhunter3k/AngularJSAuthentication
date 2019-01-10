using System;
using System.Collections.Generic;
using System.Text;

namespace AngularASPNETCore2WebApiAuth.Api.ViewModels
{
    public class RegistrationViewModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string PhoneNumber { get; set; }

        public string DisplayName { get; set; }
    }
}
