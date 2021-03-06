﻿using ASOS.Identity.Api.Entities;
using ASOS.Identity.Api.Services;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using nH.Identity.Core;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASOS.Identity.Api.Providers
{
    public class AuthorizationProvider : OpenIdConnectServerProvider
    {
        public override async Task ValidateAuthorizationRequest(ValidateAuthorizationRequestContext context)
        {
            var clientApplicationStore = context.HttpContext.RequestServices.GetRequiredService<IClientApplicationStore>();
            // Note: the OpenID Connect server middleware supports the authorization code,
            // implicit/hybrid and custom flows but this authorization provider only accepts
            // response_type=code authorization requests. You may consider relaxing it to support
            // the implicit or hybrid flows. In this case, consider adding checks rejecting
            // implicit/hybrid authorization requests when the client is a confidential application.
            if (!context.Request.IsAuthorizationCodeFlow())
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedResponseType,
                    description: "Only the authorization code flow is supported by this server.");
                return;
            }

            // Note: redirect_uri is not required for pure OAuth2 requests
            // but this provider uses a stricter policy making it mandatory,
            // as required by the OpenID Connect core specification.
            // See http://openid.net/specs/openid-connect-core-1_0.html#AuthRequest.
            if (string.IsNullOrEmpty(context.RedirectUri))
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidRequest,
                    description: "The required redirect_uri parameter was missing.");
                return;
            }

            // Retrieve the application details corresponding to the requested client_id.
            var application = await clientApplicationStore.GetClientApplicationAsync(context.ClientId, context.HttpContext.RequestAborted);
            if (application == null)
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "Application not found in the database: " +
                                 "ensure that your client_id is correct.");
                return;
            }
            // Note: the comparison doesn't need to be time-constant as the
            // callback URL stored in the database is not a secret value.
            if (!application.AllowedRedirectUris.Any(redirectUri => redirectUri.StartsWith(context.RedirectUri, StringComparison.Ordinal)))
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "Invalid redirect_uri.");
                return;
            }
            context.Validate();
        }

        public override async Task ValidateTokenRequest(ValidateTokenRequestContext context)
        {
            var clientApplicationStore = context.HttpContext.RequestServices.GetRequiredService<IClientApplicationStore>();
            // Reject the token request that don't use grant_type=password or grant_type=refresh_token.
            if (!context.Request.IsAuthorizationCodeGrantType() && !context.Request.IsPasswordGrantType() && !context.Request.IsRefreshTokenGrantType())
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    description: "Only resource owner password credentials, authorization code and refresh token " +
                                 "are accepted by this authorization server");
                return;
            }
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
            var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            var userManager = signInManager.UserManager;
            // Only handle grant_type=password requests and let ASOS
            // process grant_type=refresh_token requests automatically.
            if (context.Request.IsPasswordGrantType())
            {
                var user = await userManager.FindByEmailAsync(context.Request.Username);
                if (user == null)
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Invalid credentials.");
                    return;
                }
                // Ensure the user is allowed to sign in.
                if (!await signInManager.CanSignInAsync(user))
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "The specified user is not allowed to sign in.");
                    return;
                }
                // Reject the token request if two-factor authentication has been enabled by the user.
                if (userManager.SupportsUserTwoFactor && await userManager.GetTwoFactorEnabledAsync(user))
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Two-factor authentication is required for this account.");
                    return;
                }
                // Ensure the user is not already locked out.
                if (userManager.SupportsUserLockout && await userManager.IsLockedOutAsync(user))
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Invalid credentials.");
                    return;
                }
                // Ensure the password is valid.
                if (!await userManager.CheckPasswordAsync(user, context.Request.Password))
                {
                    if (userManager.SupportsUserLockout)
                    {
                        await userManager.AccessFailedAsync(user);
                    }
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Invalid credentials.");
                    return;
                }
                if (userManager.SupportsUserLockout)
                {
                    await userManager.ResetAccessFailedCountAsync(user);
                }
                var claims = await signInManager.CreateUserPrincipalAsync(user);
                var ouathIdentity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
                // Note: the subject claim is always included in both identity and
                // access tokens, even if an explicit destination is not specified.
                ouathIdentity.AddClaim(OpenIdConnectConstants.Claims.Subject, userManager.GetUserId(claims));
                // When adding custom claims, you MUST specify one or more destinations.
                // Read "part 7" for more information about custom claims and scopes.
                ouathIdentity.AddClaim("username", userManager.GetUserName(claims),
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);
                var customClaim = claims.FindFirstValue("ASOS_Claim");
                if (customClaim != null)
                {
                    ouathIdentity.AddClaim("ASOS_Claim", customClaim,
                      OpenIdConnectConstants.Destinations.AccessToken,
                      OpenIdConnectConstants.Destinations.IdentityToken);
                }
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

        public override Task ApplyTokenResponse(ApplyTokenResponseContext context)
        {
            context.HttpContext.Response.Cookies.Append("bearer", context.Response.AccessToken);
            return Task.CompletedTask;
        }

        public override Task MatchEndpoint(MatchEndpointContext context)
        {
            // Note: by default, the OIDC server middleware only handles authorization requests made to
            // AuthorizationEndpointPath. This handler uses a more relaxed policy that allows extracting
            // authorization requests received at /connect/authorize/accept and /connect/authorize/deny.
            if (context.Options.AuthorizationEndpointPath.HasValue &&
                context.Request.Path.StartsWithSegments(context.Options.AuthorizationEndpointPath))
            {
                context.MatchAuthorizationEndpoint();
            }
            return Task.FromResult(0);
        }
    }
}
