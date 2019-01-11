using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngularASPNETCore2WebApiAuth.Api.Auth
{
    public class HasRoleRequirement : IAuthorizationRequirement
    {
        public HasRoleRequirement(string role)
        {
            Role = role;
        }

        public string Role { get; }
    }
}
