using ASOS.Identity.Api.Models;
using ASOS.Identity.Api.Services;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace ASOS.Identity.Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly IClientApplicationStore _clientApplicationStore;

        public AccountController(IClientApplicationStore clientApplicationStore)
        {
            this._clientApplicationStore = clientApplicationStore;
        }

        [Authorize, HttpGet("~/connect/authorize")]
        public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
        {
            // Extract the authorization request from the ASP.NET context.
            var request = HttpContext.GetOpenIdConnectRequest();
            // Note: ASOS implicitly ensures that an application corresponds to the client_id
            // specified in the authorization request by calling ValidateAuthorizationRequest.
            // In theory, this null check shouldn't be needed, but a race condition could occur
            // if you manually removed the application from the database after the initial check.
            var application = await _clientApplicationStore.GetClientApplicationAsync(request.ClientId, cancellationToken);
            if (application == null)
            {
                return View("Reject", new RejectViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client " +
                                       "application cannot be found in the database"
                });
            }
            return View(new AuthorizeViewModel
            {
                ApplicationName = application.Id,
                Parameters = new Dictionary<string, string>(request.GetParameters().Select(r => new KeyValuePair<string, string>(r.Key, (string)r.Value))),
                Scope = request.Scope
            });
        }

        [Authorize, HttpPost("~/connect/authorize/accept"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(CancellationToken cancellationToken)
        {
            var request = HttpContext.GetOpenIdConnectRequest();
            // Create a new ClaimsIdentity containing the claims that
            // will be used to create an id_token, a token or a code.
            var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
            // Copy the unique identifier associated with the logged-in user to the new identity.
            // Note: the subject is always included in both identity and access tokens,
            // even if an explicit destination is not explicitly specified.
            identity.AddClaim(OpenIdConnectConstants.Claims.Subject,
                User.GetClaim(OpenIdConnectConstants.Claims.Subject));
            var application = await _clientApplicationStore.GetClientApplicationAsync(request.ClientId, cancellationToken);
            if (application == null)
            {
                return View("Error", new RejectViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client " +
                                       "application cannot be found in the database"
                });
            }
            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                OpenIdConnectServerDefaults.AuthenticationScheme);
            // Set the list of scopes granted to the client application.
            // Note: this sample always grants the "openid", "email" and "profile" scopes
            // when they are requested by the client application: a real world application
            // would probably display a form allowing to select the scopes to grant.
            ticket.SetScopes(
                /* openid: */ OpenIdConnectConstants.Scopes.OpenId,
                /* email: */ OpenIdConnectConstants.Scopes.Email,
                /* profile: */ OpenIdConnectConstants.Scopes.Profile);
            // Set the resource servers the access token should be issued for.
            ticket.SetResources("resource_server");
            // Returning a SignInResult will ask ASOS to serialize the specified identity
            // to build appropriate tokens. You should always make sure the identities
            // you return contain the OpenIdConnectConstants.Claims.Subject claim. In this sample,
            // the identity always contains the name identifier returned by the external provider.
            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        [Authorize]
        [HttpPost("~/connect/authorize/deny")]
        [ValidateAntiForgeryToken]
        public IActionResult Deny()
        {
            // Notify ASOS that the authorization grant has been denied by the resource owner.
            // The user agent will be redirected to the client application as part of this call.
            return Forbid(OpenIdConnectServerDefaults.AuthenticationScheme);
        }
    }
}
