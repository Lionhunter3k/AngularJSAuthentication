using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngularASPNETCore2WebApiAuth.Api.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using nH.Identity.Core;
using NHibernate;
using NHibernate.Linq;

namespace AngularASPNETCore2WebApiAuth.Api.Auth
{
    public class RefreshTokenFactory : IRefreshTokenFactory
    {
        private readonly NHibernate.ISession _session;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RefreshTokenOptions _refreshTokenOptions;

        public RefreshTokenFactory(NHibernate.ISession session, IHttpContextAccessor httpContextAccessor, IOptions<RefreshTokenOptions> refreshTokenOptions)
        {
            _session = session;
            _httpContextAccessor = httpContextAccessor;
            _refreshTokenOptions = refreshTokenOptions.Value;
        }

        public async Task<string> GenerateTokenAsync(string accessToken, User user, string clientId)
        {
            var refreshToken = new RefreshToken(accessToken, DateTime.UtcNow.AddDays(_refreshTokenOptions.DaysToExpire), user, _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString(), clientId);
            await _session.SaveAsync(refreshToken);
            await _session.FlushAsync();
            return refreshToken.Id.ToString("N");
        }

        public async Task<(User User, string Token)?> RetrieveTokenAsync(string token, string clientId)
        {
            var guid = Guid.ParseExact(token, "N");
            var refreshToken = await _session.Query<RefreshToken>().SingleOrDefaultAsync(q => q.ClientId == clientId && q.Id == guid && q.ExpiresUtc >= DateTime.UtcNow);
            if (refreshToken == null)
                return null;
            await _session.DeleteAsync(refreshToken);
            return (refreshToken.User, refreshToken.Token);
        }
    }
}
