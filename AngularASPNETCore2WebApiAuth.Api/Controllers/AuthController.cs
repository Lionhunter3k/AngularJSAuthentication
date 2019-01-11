using AngularASPNETCore2WebApiAuth.Api.Auth;
using AngularASPNETCore2WebApiAuth.Api.Extensions;
using AngularASPNETCore2WebApiAuth.Api.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using nH.Identity.Core;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AngularASPNETCore2WebApiAuth.Api.Controllers
{
    [Route("token")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtFactory _jwtFactory;
        private readonly IRefreshTokenFactory _refreshTokenFactory;

        public AuthController(UserManager<User> userManager, IJwtFactory jwtFactory, IRefreshTokenFactory refreshTokenFactory)
        {
            _userManager = userManager;
            _jwtFactory = jwtFactory;
            _refreshTokenFactory = refreshTokenFactory;
        }

        // POST token
        [HttpPost]
        public async Task<IActionResult> Login([FromForm]CredentialsViewModel credentials)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (credentials.GrantType == "password")
            {
                // get the user to verifty
                var userToVerify = await _userManager.FindByEmailAsync(credentials.UserName);

                if (userToVerify != null)
                {
                    // check the credentials
                    if (await _userManager.CheckPasswordAsync(userToVerify, credentials.Password))
                    {
                        var cp = await _jwtFactory.GetPrincipalFromUserAsync(userToVerify);
                        return await GenerateTokens(credentials.ClientId, userToVerify, cp);
                    }
                }

                ModelState.AddModelError("login_failure", "Invalid email or password.");
                return BadRequest(ModelState);
            }
            if(credentials.GrantType == "refresh_token")
            {
                var token = await _refreshTokenFactory.RetrieveTokenAsync(credentials.RefreshToken, credentials.ClientId);
                if(token != null)
                {
                    var cp = await _jwtFactory.GetPrincipalFromTokenAsync(token.Item2);
                    if (cp != null)
                    {
                        return await GenerateTokens(credentials.ClientId, token.Item1, cp);
                    }

                }
                ModelState.AddModelError("login_failure", "Refresh token not found or invalid.");
                return BadRequest(ModelState);
            }
            ModelState.AddModelError("login_failure", "Invalid grant type.");
            return BadRequest(ModelState);
        }

        private async Task<IActionResult> GenerateTokens(string clientId, User user, ClaimsPrincipal claimsPrincipal)
        {
            var accessToken = await _jwtFactory.GenerateEncodedTokenAsync(claimsPrincipal);
            string refresh_token = null;
            if (!string.IsNullOrEmpty(clientId))
            {
                refresh_token = await _refreshTokenFactory.GenerateTokenAsync(accessToken.Value, user, clientId);
            }
            var response = new
            {
                access_token = accessToken.Value,
                expires_in = accessToken.ExpiresInSeconds,
                refresh_token
            };
            return new JsonResult(response);
        }
    }
}
