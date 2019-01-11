using nH.Identity.Core;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AngularASPNETCore2WebApiAuth.Api.Auth
{
    public interface IJwtFactory
    {
        Task<AccessToken> GenerateEncodedTokenAsync(ClaimsPrincipal user);

        Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token);

        Task<ClaimsPrincipal> GetPrincipalFromUserAsync(User user);
    }
}
