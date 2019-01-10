using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;

namespace TokenApi.Extensions
{
    //[Authorize(Policy ="admin")]
    //[Authorize(Policy = "RequireAdministratorRole")]
    [Authorize(Roles = "ADMIN")]  // case sensitive!
    //[Authorize]
    [Route("api/protected")]
    public class ProtectedController : ControllerBase
    {
        public IEnumerable<object> Get()
        {
            //string token = "";
            //Microsoft.Owin.Security.AuthenticationTicket ticket = Startup.OAuthBearerOptions.AccessTokenFormat.Unprotect(token);

            return User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            });
        }
    }
}
