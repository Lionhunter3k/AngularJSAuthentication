using ASOS.Identity.Api.Entities;
using ASOS.Identity.Api.Services;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using nH.Identity.Core;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASOS.Identity.Api.Providers
{
    public class AuthorizationProvider : OpenIdConnectServerProvider
    {
        public override async Task ValidateTokenRequest(ValidateTokenRequestContext context)
        {
            var clientApplicationStore = context.HttpContext.RequestServices.GetRequiredService<IClientApplicationStore>();
            // Skip client authentication if the client identifier is missing.
            // Note: ASOS will automatically ensure that the calling application
            // cannot use an authorization code or a refresh token if it's not
            // the intended audience, even if client authentication was skipped.
            if (string.IsNullOrEmpty(context.ClientId))
            {
                context.Skip();
                return;
            }
            // Retrieve the application details corresponding to the requested client_id.
            var application = await clientApplicationStore.GetClientApplicationAsync(context.ClientId, context.HttpContext.RequestAborted);
            if (application == null)
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "Application not found in the database: ensure that your client_id is correct.");
                return;
            }
            // Reject the token request that don't use grant_type=password or grant_type=refresh_token.
            if (!context.Request.IsPasswordGrantType() && !context.Request.IsRefreshTokenGrantType())
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    description: "Only resource owner password credentials and refresh token " +
                                 "are accepted by this authorization server");
                return;
            }
            if (!application.AllowedGrants.Contains(context.Request.GrantType))
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnauthorizedClient,
                    description: "Client is not the allowed the specified grant type");
                return;
            }
            if (application.Type == ApplicationType.Public)
            {
                // Reject tokens requests containing a client_secret
                // if the client application is not confidential.
                if (!string.IsNullOrEmpty(context.ClientSecret))
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidRequest,
                        description: "Public clients are not allowed to send a client_secret.");
                    return;
                }
                // If client authentication cannot be enforced, call context.Skip() to inform
                // the OpenID Connect server middleware that the caller cannot be fully trusted.
                context.Skip();
                return;
            }
            // Confidential applications MUST authenticate
            // to protect them from impersonation attacks.
            if (string.IsNullOrEmpty(context.ClientSecret))
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "Missing credentials: ensure that you specified a client_secret.");
                return;
            }
            // Note: to mitigate brute force attacks, you SHOULD strongly consider applying
            // a key derivation function like PBKDF2 to slow down the secret validation process.
            // You SHOULD also consider using a time-constant comparer to prevent timing attacks.
            // For that, you can use the CryptoHelper library developed by @henkmollema:
            // https://github.com/henkmollema/CryptoHelper. If you don't need .NET Core support,
            // SecurityDriven.NET/inferno is a rock-solid alternative: http://securitydriven.net/inferno/
            if (!string.Equals(context.ClientSecret, application.Secret, StringComparison.Ordinal))
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "Invalid credentials: ensure that you specified a correct client_secret.");
                return;
            }
            context.Validate();
        }

        public override async Task HandleTokenRequest(HandleTokenRequestContext context)
        {
            // Resolve ASP.NET Core Identity's user manager from the DI container.
            var manager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            // Only handle grant_type=password requests and let ASOS
            // process grant_type=refresh_token requests automatically.
            if (context.Request.IsPasswordGrantType())
            {
                var user = await manager.UserManager.FindByNameAsync(context.Request.Username);
                if (user == null)
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Invalid credentials.");
                    return;
                }
                // Ensure the user is allowed to sign in.
                if (!await manager.CanSignInAsync(user))
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "The specified user is not allowed to sign in.");
                    return;
                }
                // Reject the token request if two-factor authentication has been enabled by the user.
                if (manager.UserManager.SupportsUserTwoFactor && await manager.UserManager.GetTwoFactorEnabledAsync(user))
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Two-factor authentication is required for this account.");
                    return;
                }
                // Ensure the user is not already locked out.
                if (manager.UserManager.SupportsUserLockout && await manager.UserManager.IsLockedOutAsync(user))
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Invalid credentials.");
                    return;
                }
                // Ensure the password is valid.
                if (!await manager.UserManager.CheckPasswordAsync(user, context.Request.Password))
                {
                    if (manager.UserManager.SupportsUserLockout)
                    {
                        await manager.UserManager.AccessFailedAsync(user);
                    }
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Invalid credentials.");
                    return;
                }
                if (manager.UserManager.SupportsUserLockout)
                {
                    await manager.UserManager.ResetAccessFailedCountAsync(user);
                }
                var claims = await manager.CreateUserPrincipalAsync(user);
                var ouathIdentity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
                // Note: the subject claim is always included in both identity and
                // access tokens, even if an explicit destination is not specified.
                ouathIdentity.AddClaim(OpenIdConnectConstants.Claims.Subject, manager.UserManager.GetUserId(claims));
                // When adding custom claims, you MUST specify one or more destinations.
                // Read "part 7" for more information about custom claims and scopes.
                ouathIdentity.AddClaim("username", manager.UserManager.GetUserName(claims),
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);
                // Create a new authentication ticket holding the user identity.
                var ticket = new AuthenticationTicket(
                    new ClaimsPrincipal(ouathIdentity),
                    new AuthenticationProperties(),
                    OpenIdConnectServerDefaults.AuthenticationScheme);
                // Set the list of scopes granted to the client application.
                ticket.SetScopes(
                    /* openid: */ OpenIdConnectConstants.Scopes.OpenId,
                    /* email: */ OpenIdConnectConstants.Scopes.Email,
                    /* profile: */ OpenIdConnectConstants.Scopes.Profile);
                // Set the resource servers the access token should be issued for.
                ticket.SetResources("resource_server");
                context.Validate(ticket);
            }
        }
    }
}
