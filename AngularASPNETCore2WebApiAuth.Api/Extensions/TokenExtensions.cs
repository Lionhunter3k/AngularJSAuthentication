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
        public static JsonSerializerSettings TokenSerializerSettings { get; set; }

        public static async Task<IActionResult> GenerateJwt(this IJwtFactory jwtFactory, ClaimsIdentity identity, string userName, JwtIssuerOptions jwtOptions)
        {
            var response = new
            {
                id = identity.FindFirst(c => c.Type == "id").Value,
                access_token = await jwtFactory.GenerateEncodedToken(userName, identity),
                expires_in = (int)jwtOptions.ValidFor.TotalSeconds
            };

            var serializedResponse = TokenSerializerSettings != null ? new JsonResult(response, TokenSerializerSettings) : new JsonResult(response);

            return serializedResponse;
        }
    }
}
