using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using nH.Identity.Core;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TokenApi.Entities;
using TokenApi.Extensions;

namespace TokenApi.Auth
{
    public class JwtFactory : IJwtFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ISession _session;

        public JwtFactory(IConfiguration configuration, ISession session)
        {
            this._configuration = configuration;
            this._session = session;
        }

        private TokenProviderOptions GetOptions()
        {
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection("TokenAuthentication:SecretKey").Value));

            return new TokenProviderOptions
            {
                Path = _configuration.GetSection("TokenAuthentication:TokenPath").Value,
                Audience = _configuration.GetSection("TokenAuthentication:Audience").Value,
                Issuer = _configuration.GetSection("TokenAuthentication:Issuer").Value,
                Expiration = TimeSpan.FromMinutes(Convert.ToInt32(_configuration.GetSection("TokenAuthentication:ExpirationMinutes").Value)),
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            };
        }

        public async Task<LoginResponseData> GenerateEncodedTokenAsync(User user, string refreshTokenType)
        {
            var options = GetOptions();
            var now = DateTime.UtcNow;

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            };

            var userClaims = user.UserClaims;
            foreach (var userClaim in userClaims)
            {
                claims.Add(new Claim(userClaim.ClaimType, userClaim.ClaimValue));
            }
            var userRoles = user.Roles;
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(AuthExtensions.RoleClaimType, userRole.Name));
            }
            string refreshTokenValue = null;
            if (!string.IsNullOrEmpty(refreshTokenType))
            {
                var refreshToken = new RefreshToken
                {
                    User = user,
                    Token = Guid.NewGuid().ToString("N"),
                    IssuedUtc = now,
                    ExpiresUtc = now.Add(options.Expiration),
                    Type = refreshTokenType
                };
                await _session.SaveAsync(refreshToken);
                await _session.FlushAsync();
                refreshTokenValue = refreshToken.Token;
            }

            var jwt = new JwtSecurityToken(
                issuer: options.Issuer,
                audience: options.Audience,
                claims: claims.ToArray(),
                notBefore: now,
                expires: now.Add(options.Expiration),
                signingCredentials: options.SigningCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new LoginResponseData
            {
                access_token = encodedJwt,
                refresh_token = refreshTokenValue,
                expires_in = (int)options.Expiration.TotalSeconds,
                userName = user.DisplayName,
                isAdmin = claims.Any(i => i.Type == AuthExtensions.RoleClaimType && i.Value == AuthExtensions.AdminRole)
            };
            return response;
        }

        public async Task<User> GetUserAsync(string refreshTokenValue)
        {
            var refreshToken = await _session.Query<RefreshToken>().Where(q => q.Token == refreshTokenValue).Fetch(q => q.User).SingleOrDefaultAsync();
            if (refreshToken != null)
            {
                await _session.DeleteAsync(refreshToken);
            }
            return refreshToken?.User;
        }
    }
}
