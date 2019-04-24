using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace OAuthTutorial.Providers
{
    public class TicketCounter
    {
        internal Task<AuthenticationTicket> MakeClaimsForClientCredentialsAsync(string clientId)
        {
            throw new NotImplementedException();
        }
    }
}