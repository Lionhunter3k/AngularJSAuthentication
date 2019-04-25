using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Identity;
using nH.Identity.Core;
using NHibernate;
using NHibernate.Linq;
using OAuthTutorial.Entities;

namespace OAuthTutorial.Services
{
    public class TokenService
    {
        private readonly ISession _session;
        private readonly UserManager<User> _userManager;

        public TokenService(ISession session, UserManager<User> userManager)
        {
            _session = session;
            _userManager = userManager;
        }

        public async Task WriteNewTokenToDatabaseAsync(string clientId, Token token, ClaimsPrincipal user = null)
        {
            if (String.IsNullOrWhiteSpace(clientId) || token == null || String.IsNullOrWhiteSpace(token.GrantType) || String.IsNullOrWhiteSpace(token.Value))
            {
                return;
            }

            var client = await _session.GetAsync<OAuthClient>(clientId);
            if (client == null)
            {
                return;
            }
            else
            {
                token.Client = client;
            }

            // Handling Client Creds
            if (token.GrantType == OpenIdConnectConstants.GrantTypes.ClientCredentials)
            {
                List<Token> OldClientCredentialTokens = await _session.Query<Token>()
                    .Where(q => q.Client == client)
                    .Where(x => x.GrantType == OpenIdConnectConstants.GrantTypes.ClientCredentials).ToListAsync();
                foreach (var old in OldClientCredentialTokens)
                {
                    await _session.DeleteAsync(old);
                }
                await _session.SaveAsync(token);
                await _session.FlushAsync();
            }
            // Handling the other flows
            else if (token.GrantType == OpenIdConnectConstants.GrantTypes.Implicit || token.GrantType == OpenIdConnectConstants.GrantTypes.AuthorizationCode || token.GrantType == OpenIdConnectConstants.GrantTypes.RefreshToken)
            {
                if (user == null)
                {
                    return;
                }
                User au = await _userManager.GetUserAsync(user);
                if (au == null)
                {
                    return;
                }
                else
                {
                    token.User = au;
                }

                // These tokens also require association to a specific user
                IEnumerable<Token> OldTokensForGrantType = await _session.Query<Token>()
                    .Where(q => q.Client == client)
                    .Where(x => x.GrantType == token.GrantType && x.TokenType == token.TokenType && x.User == au).ToListAsync();
                foreach (var old in OldTokensForGrantType)
                {
                    await _session.DeleteAsync(old);
                }
                await _session.SaveAsync(token);
                await _session.FlushAsync();
            }
        }
    }
}