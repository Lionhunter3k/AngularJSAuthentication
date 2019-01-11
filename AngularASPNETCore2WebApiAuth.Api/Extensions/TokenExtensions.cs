using AngularASPNETCore2WebApiAuth.Api.Auth;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AngularASPNETCore2WebApiAuth.Api.Extensions
{
    public static class TokenExtensions
    {
        public static class JwtClaimIdentifiers
        {
            public readonly static string Rol = "rol", Id = "id";
        }

        public static class JwtClaims
        {
            public readonly static string ApiAccess = "api_access";
        }
    }
}
