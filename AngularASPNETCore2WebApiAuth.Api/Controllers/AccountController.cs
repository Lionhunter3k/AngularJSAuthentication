using AngularASPNETCore2WebApiAuth.Api.Auth;
using AngularASPNETCore2WebApiAuth.Api.Extensions;
using AngularASPNETCore2WebApiAuth.Api.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using nH.Identity.Core;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AngularASPNETCore2WebApiAuth.Api.Controllers
{
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly NHibernate.ISession _session;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IJwtFactory _jwtFactory;
        private readonly SignInManager<User> _signInManager;

        public AccountController(ISession session, IMapper mapper, UserManager<User> userManager, IConfiguration configuration, IJwtFactory jwtFactory, SignInManager<User> signInManager)
        {
            _session = session;
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
            _jwtFactory = jwtFactory;
            _signInManager = signInManager;
        }

        // POST api/account/register
        [HttpPost]
        [Route("register")] // /api/account/register
        public async Task<IActionResult> Register([FromBody]RegistrationViewModel model)
        {
            using (var tx = _session.BeginTransaction())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdentity = _mapper.Map<User>(model);

                var result = await _userManager.CreateAsync(userIdentity, model.Password);
                if (!result.Succeeded)
                    return new BadRequestObjectResult(ModelState.AddErrorsToModelState(result));
                result = await _userManager.AddToRoleAsync(userIdentity, TokenExtensions.JwtClaims.ApiAccess);
                if (!result.Succeeded)
                    return new BadRequestObjectResult(ModelState.AddErrorsToModelState(result));
                await tx.CommitAsync();
                return new OkObjectResult("Account created");
            }
        }

        // GET api/Account/ExternalLogin
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IActionResult> GetExternalLogin(string provider, string error = null)
        {
            string redirectUri = string.Empty;

            if (error != null)
            {
                return BadRequest(Uri.EscapeDataString(error));
            }

            var redirectUriValidationResult = ValidateClientAndRedirectUri(ref redirectUri);

            if (!string.IsNullOrWhiteSpace(redirectUriValidationResult))
            {
                return BadRequest(redirectUriValidationResult);
            }

            if (!User.Identity.IsAuthenticated)
            {
                var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { redirect_uri = redirectUri });
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return Challenge(properties, provider);
            }

            var externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return base.StatusCode(500);
            }

            if (externalLogin.LoginProvider != provider)
            {
                await _signInManager.SignOutAsync();
                var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { redirect_uri = redirectUri });
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return Challenge(properties, provider);
            }

            var user = await _userManager.FindByLoginAsync(externalLogin.LoginProvider, externalLogin.ProviderKey);

            bool hasRegistered = user != null;

            redirectUri = string.Format("{0}#external_access_token={1}&provider={2}&haslocalaccount={3}&external_user_name={4}",
                                            redirectUri,
                                            externalLogin.ExternalAccessToken,
                                            externalLogin.LoginProvider,
                                            hasRegistered.ToString(),
                                            externalLogin.UserName);

            return Redirect(redirectUri);

        }

        // GET api/Account/ExternalLoginCallback
        [Route("ExternalLoginCallback", Name = "ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string provider, string error = null)
        {
            string redirectUri = string.Empty;

            if (error != null)
            {
                return BadRequest(Uri.EscapeDataString(error));
            }

            var redirectUriValidationResult = ValidateClientAndRedirectUri(ref redirectUri);

            if (!string.IsNullOrWhiteSpace(redirectUriValidationResult))
            {
                return BadRequest(redirectUriValidationResult);
            }

            var externalLogin = await _signInManager.GetExternalLoginInfoAsync();

            if (externalLogin == null)
            {
                return base.StatusCode(500);
            }

            var user = await _userManager.FindByLoginAsync(externalLogin.LoginProvider, externalLogin.ProviderKey);

            bool hasRegistered = user != null;

            redirectUri = string.Format("{0}#external_access_token={1}&provider={2}&haslocalaccount={3}&external_user_name={4}",
                                            redirectUri,
                                            externalLogin.Principal.FindFirstValue("externalAccessToken"),
                                            externalLogin.LoginProvider,
                                            hasRegistered.ToString(),
                                            externalLogin.Principal.FindFirstValue(ClaimTypes.Email));

            return Redirect(redirectUri);

        }

        // POST api/Account/RegisterExternal
        [HttpPost]
        [Route("RegisterExternal")]
        public async Task<IActionResult> RegisterExternal([FromBody]RegisterExternalBindingModel model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var verifiedAccessToken = await VerifyExternalAccessTokenAsync(model.Provider, model.ExternalAccessToken);
            if (verifiedAccessToken == null)
            {
                return BadRequest("Invalid Provider or External Access Token");
            }
            var user = await _userManager.FindByLoginAsync(model.Provider, verifiedAccessToken.user_id);
            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                return BadRequest("External user is already registered");
            }

            user = new User { DisplayName = model.UserName, Email = model.UserName, PhoneNumber = model.PhoneNumber };
            using (var tx = _session.BeginTransaction())
            {
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return new BadRequestObjectResult(ModelState.AddErrorsToModelState(result));
                }

                var info = new UserLoginInfo(model.Provider, verifiedAccessToken.user_id, model.UserName);

                result = await _userManager.AddLoginAsync(user, info);
                if (!result.Succeeded)
                {
                    return new BadRequestObjectResult(ModelState.AddErrorsToModelState(result));
                }
                result = await _userManager.AddToRoleAsync(user, TokenExtensions.JwtClaims.ApiAccess);
                if (!result.Succeeded)
                    return new BadRequestObjectResult(ModelState.AddErrorsToModelState(result));
                await tx.CommitAsync();
                //generate access token response
                var accessTokenResponse = await GenerateLocalAccessTokenResponseAsync(user);

                return Ok(accessTokenResponse);
            }
        }

        [HttpGet]
        [Route("ObtainLocalAccessToken")]
        public async Task<IActionResult> ObtainLocalAccessToken(string provider, string externalAccessToken)
        {

            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(externalAccessToken))
            {
                return BadRequest("Provider or external access token is not sent");
            }

            var verifiedAccessToken = await VerifyExternalAccessTokenAsync(provider, externalAccessToken);
            if (verifiedAccessToken == null)
            {
                return BadRequest("Invalid Provider or External Access Token");
            }

            var user = await _userManager.FindByLoginAsync(provider, verifiedAccessToken.user_id);

            bool hasRegistered = user != null;

            if (!hasRegistered)
            {
                return BadRequest("External user is not registered");
            }

            //generate access token response
            var accessTokenResponse = await GenerateLocalAccessTokenResponseAsync(user);

            return Ok(accessTokenResponse);

        }

        private string ValidateClientAndRedirectUri(ref string redirectUriOutput)
        {
            var redirectUriString = GetQueryString("redirect_uri");

            if (string.IsNullOrWhiteSpace(redirectUriString))
            {
                return "redirect_uri is required";
            }

            bool validUri = Uri.TryCreate(redirectUriString, UriKind.Absolute, out Uri redirectUri);

            if (!validUri)
            {
                return "redirect_uri is invalid";
            }

            var clientId = GetQueryString("client_id");

            if (string.IsNullOrWhiteSpace(clientId))
            {
                //return "client_Id is required";
            }

            redirectUriOutput = redirectUri.AbsoluteUri;

            return string.Empty;

        }

        private string GetQueryString(string key)
        {
            var queryStrings = HttpContext.Request.Query;

            if (queryStrings == null) return null;

            var match = queryStrings.FirstOrDefault(keyValue => string.Compare(keyValue.Key, key, true) == 0);

            if (string.IsNullOrEmpty(match.Value)) return null;

            return match.Value;
        }

        private async Task<ParsedExternalAccessToken> VerifyExternalAccessTokenAsync(string provider, string accessToken)
        {
            ParsedExternalAccessToken parsedToken = null;

            var verifyTokenEndPoint = "";

            if (provider == "Facebook")
            {
                //You can get it from here: https://developers.facebook.com/tools/accesstoken/
                //More about debug_tokn here: http://stackoverflow.com/questions/16641083/how-does-one-get-the-app-access-token-for-debug-token-inspection-on-facebook
                var appToken = _configuration["Authentication:Facebook:AppToken"];
                verifyTokenEndPoint = string.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}", accessToken, appToken);
            }
            else if (provider == "Google")
            {
                verifyTokenEndPoint = string.Format("https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={0}", accessToken);
            }
            else
            {
                return null;
            }

            using (var client = new HttpClient())
            { 
                var uri = new Uri(verifyTokenEndPoint);
                var response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    JObject jObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                    parsedToken = new ParsedExternalAccessToken();

                    if (provider == "Facebook")
                    {
                        parsedToken.user_id = jObj["data"]["user_id"].ToString();
                        parsedToken.app_id = jObj["data"]["app_id"].ToString();

                        if (!string.Equals(_configuration["Authentication:Facebook:AppId"], parsedToken.app_id, StringComparison.OrdinalIgnoreCase))
                        {
                            return null;
                        }
                    }
                    else if (provider == "Google")
                    {
                        parsedToken.user_id = jObj["user_id"].ToString();
                        parsedToken.app_id = jObj["audience"].ToString();

                        if (!string.Equals(_configuration["Authentication:Google:AppId"], parsedToken.app_id, StringComparison.OrdinalIgnoreCase))
                        {
                            return null;
                        }

                    }
                }
            }

            return parsedToken;
        }

        private async Task<JObject> GenerateLocalAccessTokenResponseAsync(User user)
        {
            var identity = await _jwtFactory.GetPrincipalFromUserAsync(user);

            var accessToken = await _jwtFactory.GenerateEncodedTokenAsync(identity);

            var tokenResponse = new JObject(
                                        new JProperty("userName", user.DisplayName),
                                        new JProperty("access_token", accessToken.Value),
                                        new JProperty("token_type", "bearer"),
                                        new JProperty("expires_in", accessToken.ExpiresInSeconds.ToString()));

            return tokenResponse;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }
            public string ExternalAccessToken { get; set; }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer) || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirst(ClaimTypes.Name)?.Value,
                    ExternalAccessToken = identity.FindFirst("ExternalAccessToken")?.Value,
                };
            }
        }
    }
}
