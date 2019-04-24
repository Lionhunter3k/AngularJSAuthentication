using AspNet.Security.OpenIdConnect.Primitives;
using NHibernate;
using NHibernate.Linq;
using OAuthTutorial.Entities;
using OAuthTutorial.Services;
using System;
using System.Threading.Tasks;

namespace OAuthTutorial.Providers
{
    public class ValidationService
    {
        private readonly ISession _session;

        public ValidationService(ISession session)
        {
            _session = session;
        }

        public async Task<bool> CheckClientIdIsValidAsync(string clientid)
        {
            if (String.IsNullOrWhiteSpace(clientid))
            {
                return false;
            }
            else
            {
                return (await _session.GetAsync<OAuthClient>(clientid)) != null;
            }
        }

        public async Task<bool> CheckRedirectURIMatchesClientIdAsync(string clientid, string rdi)
        {
            if (String.IsNullOrWhiteSpace(clientid) || String.IsNullOrWhiteSpace(rdi))
            {
                return false;
            }
            return ((await _session.GetAsync<OAuthClient>(clientid))?.RedirectURIs.Contains(rdi)).GetValueOrDefault();
        }

        public Task<bool> CheckScopesAreValidAsync(string scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                return Task.FromResult(true); // Unlike the other checks, an empty scope is a valid scope. It just means the application has default permissions.
            }

            string[] scopes = scope.Split(' ');
            foreach (string s in scopes)
            {
                if (!OAuthScope.NameInScopes(s))
                {
                    return Task.FromResult(false);
                }
            }
            return Task.FromResult(true);
        }

        public async Task<bool> CheckClientIdAndSecretIsValidAsync(string clientId, string clientSecret)
        {
            if (String.IsNullOrWhiteSpace(clientId) || String.IsNullOrWhiteSpace(clientSecret))
            {
                return false;
            }
            else
            {
                // This could be an easy check, but the ASOS maintainer strongly recommends you to use a fixed-time string compare for client secrets.
                // This is trivially available in any .NET Core 2.1 or higher framework, but this is a 2.0 project, so we will leave that part out.
                // If you are on 2.1+, checkout the System.Security.Cryptography.CryptographicOperations.FixedTimeEquals() mehod,
                // available at https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.cryptographicoperations.fixedtimeequals?view=netcore-2.1
                return (await _session.GetAsync<OAuthClient>(clientId))?.ClientSecret == clientSecret;
            }
        }

        public async Task<bool> CheckRefreshTokenIsValidAsync(string refreshToken)
        {
            if (String.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }
            else
            {
                return await _session.Query<Token>().AnyAsync(y => y.TokenType == OpenIdConnectConstants.TokenUsages.RefreshToken && y.Value == refreshToken);
            }
        }
    }
}