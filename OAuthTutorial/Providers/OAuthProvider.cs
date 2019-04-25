using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using NHibernate;
using OAuthTutorial.Entities;
using OAuthTutorial.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OAuthTutorial.Providers
{
    public class OAuthProvider : OpenIdConnectServerProvider
    {

        // These doesn't exist yet - but they will further down.
        private readonly ValidationService VService;
        private readonly TokenService TService;
        private readonly ISession _session;
        private readonly TicketCounter TicketCounter;

        public OAuthProvider(ValidationService vService, TokenService service, ISession session, TicketCounter ticketCounter)
        {
            VService = vService;
            TService = service;
            _session = session;
            TicketCounter = ticketCounter;
        }

        public override Task MatchEndpoint(MatchEndpointContext context)
        {
            if (context.Options.AuthorizationEndpointPath.HasValue &&
                    context.Request.Path.Value.StartsWith(context.Options.AuthorizationEndpointPath))
            {
                context.MatchAuthorizationEndpoint();
            }
            return Task.CompletedTask;
        }


        #region Authorization Requests

        /*
        The supplied client id needs to exist
        The redirect uri cannot be empty and must be properly registered to the client
        If scopes are requested, they must not be bogus
         */
        public override async Task ValidateAuthorizationRequest(ValidateAuthorizationRequestContext context)
        {
            var request = context.Request;
            if (!request.IsAuthorizationCodeFlow() && !request.IsImplicitFlow())
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedResponseType,
                    description: "Only authorization code, refresh token, and token grant types are accepted by this authorization server."
                );
                return;
            }

            //string clientid = context.ClientId;
            //string rdi = request.RedirectUri;
            //string state = request.State;
            //string scope = request.Scope;

            if (String.IsNullOrWhiteSpace(request.ClientId))
            {
                context.Reject(
                            error: OpenIdConnectConstants.Errors.InvalidClient,
                            description: "client_id cannot be empty"
                        );
                return;
            }
            else if (String.IsNullOrWhiteSpace(request.RedirectUri))
            {
                context.Reject(
                            error: OpenIdConnectConstants.Errors.InvalidClient,
                            description: "redirect_uri cannot be empty"
                        );
                return;
            }
            else if (!await VService.CheckClientIdIsValidAsync(request.ClientId))
            {
                context.Reject(
                            error: OpenIdConnectConstants.Errors.InvalidClient,
                            description: "The supplied client id does not exist"
                        );
                return;
            }
            else if (!await VService.CheckRedirectURIMatchesClientIdAsync(request.ClientId, request.RedirectUri))
            {
                context.Reject(
                            error: OpenIdConnectConstants.Errors.InvalidClient,
                            description: "The supplied redirect uri is incorrect"
                        );
                return;
            }
            else if (!await VService.CheckScopesAreValidAsync(request.Scope))
            {
                context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidRequest,
                        description: "One or all of the supplied scopes are invalid"
                    );
                return;
            }

            context.Validate();
        }

        //only for implicit grant
        public override async Task ApplyAuthorizationResponse(ApplyAuthorizationResponseContext context)
        {
            if (!String.IsNullOrWhiteSpace(context.Error))
            {
                return;
            }
            var request = context.Request;
            if (!request.IsImplicitFlow())
            {
                return;
            }
            ClaimsPrincipal claimsUser = context.HttpContext.User;
            // Implicit grant is the only flow that gets their token issued here.
            var access = new Token
            {
                GrantType = OpenIdConnectConstants.GrantTypes.Implicit,
                TokenType = OpenIdConnectConstants.TokenUsages.AccessToken,
                Value = context.AccessToken,
            };

            var client = await _session.GetAsync<OAuthClient>(context.Request.ClientId);
            if (client == null)
            {
                return;
            }

            await TService.WriteNewTokenToDatabaseAsync(request.ClientId, access, claimsUser);
        }
        #endregion


        #region Token Requests
        public override async Task ValidateTokenRequest(ValidateTokenRequestContext context)
        {
            var request = context.Request;
            // We only accept "authorization_code", "refresh", "token" for this endpoint.
            if (!request.IsAuthorizationCodeGrantType()
                && !request.IsRefreshTokenGrantType()
                && !request.IsClientCredentialsGrantType())
            {
                context.Reject(
                        error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                        description: "Only authorization code, refresh token, and token grant types are accepted by this authorization server."
                    );
            }

            // Validating the Authorization Code Token Request
            if (request.IsAuthorizationCodeGrantType())
            {
                //clientid = context.ClientId;
                //clientsecret = context.ClientSecret;
                //code = context.Request.Code;
                //redirecturi = context.Request.RedirectUri;

                if (String.IsNullOrWhiteSpace(request.ClientId))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "client_id cannot be empty"
                          );
                    return;
                }
                else if (String.IsNullOrWhiteSpace(request.ClientSecret))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "client_secret cannot be empty"
                          );
                    return;
                }
                else if (String.IsNullOrWhiteSpace(request.RedirectUri))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "redirect_uri cannot be empty"
                          );
                    return;
                }
                else if (!await VService.CheckClientIdIsValidAsync(request.ClientId))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "The supplied client id was does not exist"
                          );
                    return;
                }
                else if (!await VService.CheckClientIdAndSecretIsValidAsync(request.ClientId, request.ClientSecret))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "The supplied client secret is invalid"
                          );
                    return;
                }
                else if (!await VService.CheckRedirectURIMatchesClientIdAsync(request.ClientId, request.RedirectUri))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "The supplied redirect uri is incorrect"
                          );
                    return;
                }

                context.Validate();
                return;
            }
            // Validating the Refresh Code Token Request
            else if (request.IsRefreshTokenGrantType())
            {
                //clientid = context.Request.ClientId;
                //clientsecret = context.Request.ClientSecret;
                //refreshtoken = context.Request.RefreshToken;

                if (String.IsNullOrWhiteSpace(request.ClientId))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "client_id cannot be empty"
                          );
                    return;
                }
                else if (String.IsNullOrWhiteSpace(request.ClientSecret))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "client_secret cannot be empty"
                          );
                    return;
                }
                else if (!await VService.CheckClientIdIsValidAsync(request.ClientId))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "The supplied client id does not exist"
                          );
                    return;
                }
                else if (!await VService.CheckClientIdAndSecretIsValidAsync(request.ClientId, request.ClientSecret))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "The supplied client secret is invalid"
                          );
                    return;
                }
                else if (!await VService.CheckRefreshTokenIsValidAsync(request.RefreshToken))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "The supplied refresh token is invalid"
                          );
                    return;
                }

                context.Validate();
                return;
            }
            // Validating Client Credentials Request, aka, 'token'
            else if (request.IsClientCredentialsGrantType())
            {
                //clientid = context.ClientId;
                //clientsecret = context.ClientSecret;


                if (String.IsNullOrWhiteSpace(request.ClientId))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "client_id cannot be empty"
                          );
                    return;
                }
                else if (String.IsNullOrWhiteSpace(request.ClientSecret))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "client_secret cannot be empty"
                          );
                    return;
                }
                else if (!await VService.CheckClientIdIsValidAsync(request.ClientId))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "The supplied client id does not exist"
                          );
                    return;
                }
                else if (!await VService.CheckClientIdAndSecretIsValidAsync(request.ClientId, request.ClientSecret))
                {
                    context.Reject(
                              error: OpenIdConnectConstants.Errors.InvalidClient,
                              description: "The supplied client secret is invalid"
                          );
                    return;
                }

                context.Validate();
                return;
            }
            else
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.ServerError,
                    description: "Could not validate the token request"
                );
                return;
            }
        }

        public override async Task HandleTokenRequest(HandleTokenRequestContext context)
        {
            AuthenticationTicket ticket = null;
            var request = context.Request;
            // Handling Client Credentials
            if (request.IsClientCredentialsGrantType())
            {
                // If we do not specify any form of Ticket, or ClaimsIdentity, or ClaimsPrincipal, our validation will succeed here but fail later.
                // ASOS needs those to serialize a token, and without any, it fails because there's way to fashion a token properly. Check the ASOS source for more details.
                ticket = await TicketCounter.MakeClaimsForClientCredentialsAsync(request.ClientId);
                context.Validate(ticket);
                return;
            }
            // Handling Authorization Codes
            else if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            {
                ticket = context.Ticket;
                if (ticket != null)
                {
                    context.Validate(ticket);
                    return;
                }
                else
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidRequest,
                        description: "User isn't valid"
                    );
                    return;
                }

            }
            // Catch all error
            context.Reject(
                error: OpenIdConnectConstants.Errors.ServerError,
                description: "Could not validate the token request"
            );
        }

        public override async Task ApplyTokenResponse(ApplyTokenResponseContext context)
        {
            using (var tx = _session.BeginTransaction())
            {
                if (context.Error != null)
                {
                    return;
                }
                var request = context.Request;
                var response = context.Response;
                var client = await _session.GetAsync<OAuthClient>(request.ClientId);
                if (client == null)
                {
                    return;
                }

                // Implicit Flow Tokens are not returned from the `Token` group of methods - you can find them in the `Authorize` group.
                if (request.IsClientCredentialsGrantType())
                {
                    // The only thing returned from a successful client grant is a single `Token`
                    var t = new Token
                    {
                        TokenType = OpenIdConnectConstants.TokenUsages.AccessToken,
                        GrantType = OpenIdConnectConstants.GrantTypes.ClientCredentials,
                        Value = response.AccessToken,
                    };

                    await TService.WriteNewTokenToDatabaseAsync(request.ClientId, t);
                }
                else if (context.Request.IsAuthorizationCodeGrantType())
                {
                    var access = new Token
                    {
                        TokenType = OpenIdConnectConstants.TokenUsages.AccessToken,
                        GrantType = OpenIdConnectConstants.GrantTypes.AuthorizationCode,
                        Value = response.AccessToken,
                    };
                    var refresh = new Token
                    {
                        TokenType = OpenIdConnectConstants.TokenUsages.RefreshToken,
                        GrantType = OpenIdConnectConstants.GrantTypes.AuthorizationCode,
                        Value = response.RefreshToken,
                    };

                    await TService.WriteNewTokenToDatabaseAsync(request.ClientId, access, context.Ticket.Principal);
                    await TService.WriteNewTokenToDatabaseAsync(request.ClientId, refresh, context.Ticket.Principal);
                }
                else if (context.Request.IsRefreshTokenGrantType())
                {
                    Token access = new Token
                    {
                        TokenType = OpenIdConnectConstants.TokenUsages.AccessToken,
                        GrantType = OpenIdConnectConstants.GrantTypes.AuthorizationCode,
                        Value = response.AccessToken,
                    };
                    await TService.WriteNewTokenToDatabaseAsync(request.ClientId, access, context.Ticket.Principal);
                }
                await tx.CommitAsync();
            }
        }
        #endregion
    }
}
