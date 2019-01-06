using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Providers
{
    public class AudienceProvider : IEnumerable<string>, IEnumerable<IIssuerSecurityKeyProvider>
    {
        private readonly string _issuer = string.Empty;

        public AudienceProvider(string issuer)
        {
            _issuer = issuer;
        }

        private IEnumerator<string> InternalLoadAudienceIds()
        {
            using (var authContext = new AuthRepository())
            {
                foreach(var client in authContext.GetAllAudiences())
                {
                    yield return client.Id;
                }
            }
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return InternalLoadAudienceIds();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator<IIssuerSecurityKeyProvider> IEnumerable<IIssuerSecurityKeyProvider>.GetEnumerator()
        {
            using (var authContext = new AuthRepository())
            {
                foreach (var client in authContext.GetAllAudiences())
                {
                    yield return new SymmetricKeyIssuerSecurityKeyProvider(_issuer, TextEncodings.Base64Url.Decode(client.Base64Secret));
                }
            }
        }
    }
}