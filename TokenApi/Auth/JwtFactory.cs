using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using nH.Identity.Core;
using NHibernate;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using TokenApi.Extensions;

namespace TokenApi.Auth
{
    public class JwtFactory : IJwtFactory
    {
        private readonly IConfiguration _configuration;

        public JwtFactory(IConfiguration configuration)
        {
            this._configuration = configuration;
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

        public LoginResponseData GenerateEncodedToken(User user)
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
                expires_in = (int)options.Expiration.TotalSeconds,
                userName = user.DisplayName,
                isAdmin = claims.Any(i => i.Type == AuthExtensions.RoleClaimType && i.Value == AuthExtensions.AdminRole)
            };
            return response;
        }
    }
}
