using ASOS.Identity.Api.Entities;
using Microsoft.AspNetCore.Identity;
using nH.Identity.Core;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASOS.Identity.Api.Migrations
{
    public static class ApiDbSeedData
    {
        public static async Task Seed(UserManager<User> userManager, ISession session)
        {
            using (var tx = session.BeginTransaction())
            {
                var user = new User { DisplayName = "lionhunter", Email = "lionhunter@mail.com", PhoneNumber = "31231231" };
                await userManager.CreateAsync(user, "user123456");
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("ASOS_Claim", "I like pie"));
                await session.SaveAsync(new ClientApplication { AllowedGrants = new List<string> { "password", "authorization_code" }, AllowedRedirectUris = new List<string> { "https://localhost:44324" }, Id = "ngAuthApp", Type = ApplicationType.Public });
                await tx.CommitAsync();
            }
        }
    }
}
