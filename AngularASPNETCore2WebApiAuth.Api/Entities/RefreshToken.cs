using nH.Identity.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngularASPNETCore2WebApiAuth.Api.Entities
{
    public class RefreshToken : BaseEntity<Guid>
    {
        public virtual string Token { get; protected set; }
        public virtual DateTime ExpiresUtc { get; protected set; }
        public virtual User User { get; protected set; }
        public virtual bool Active => DateTime.UtcNow <= ExpiresUtc;
        public virtual string RemoteIpAddress { get; protected set; }
        public virtual string ClientId { get; protected set; }

        public RefreshToken(string token, DateTime expires, User user, string remoteIpAddress, string clientId)
        {
            Token = token;
            ExpiresUtc = expires;
            User = user;
            RemoteIpAddress = remoteIpAddress;
            ClientId = clientId;
        }

        protected RefreshToken() { }
    }
}
