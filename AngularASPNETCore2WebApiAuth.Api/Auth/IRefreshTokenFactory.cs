using AngularASPNETCore2WebApiAuth.Api.Entities;
using nH.Identity.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AngularASPNETCore2WebApiAuth.Api.Auth
{
    public interface IRefreshTokenFactory
    {
        Task<string> GenerateTokenAsync(string accessToken, User user, string clientId);

        Task<(User User, string Token)?> RetrieveTokenAsync(string token, string clientId);
    }
}
