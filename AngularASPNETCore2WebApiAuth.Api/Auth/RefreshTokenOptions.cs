using System;
using System.Collections.Generic;
using System.Text;

namespace AngularASPNETCore2WebApiAuth.Api.Auth
{
    public class RefreshTokenOptions
    {
        public int DaysToExpire { get; set; } = 5;
    }
}
