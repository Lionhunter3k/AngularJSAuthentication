using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AngularASPNETCore2WebApiAuth.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using nH.Identity.Core;

namespace AngularASPNETCore2WebApiAuth.Api.Auth
{
    public class JwtFactory : IJwtFactory
    {
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public JwtFactory(IOptions<JwtIssuerOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _jwtOptions.SecurityKey,
                ValidateLifetime = true // we check expired tokens here,
            };
            ThrowIfInvalidOptions(_jwtOptions);
        }

        public Task<AccessToken> GenerateEncodedTokenAsync(ClaimsPrincipal user)
        {
            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: user.Claims,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials);

            var encodedJwt = _jwtSecurityTokenHandler.WriteToken(jwt);

            return Task.FromResult(new AccessToken(encodedJwt, (int)_jwtOptions.ValidFor.TotalSeconds));
        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }

        public Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token)
        {
            try
            {
                var principal = _jwtSecurityTokenHandler.ValidateToken(token, _tokenValidationParameters, out var securityToken);
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return Task.FromResult<ClaimsPrincipal>(null);

                return Task.FromResult(principal);
            }
            catch
            {
                return Task.FromResult<ClaimsPrincipal>(null);
            }
        }

        public async Task<ClaimsPrincipal> GetPrincipalFromUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                 new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                 new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                 new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                 new Claim(TokenExtensions.JwtClaimIdentifiers.Id, user.Id)
             };
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(TokenExtensions.JwtClaimIdentifiers.Rol, role.Name));
            }
            return new ClaimsPrincipal(new List<ClaimsIdentity> { new ClaimsIdentity(new GenericIdentity(user.Email, JwtBearerDefaults.AuthenticationScheme), claims) });
        }
    }
}
