using AngularASPNETCore2WebApiAuth.Api.Extensions;
using Microsoft.AspNetCore.Identity;
using nH.Identity.Core;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularASPNETCore2WebApiAuth.Api.Migrations
{
    public static class ApiDbSeedData
    {
        public static async Task Seed(RoleManager<Role> roleManager, ISession session)
        {
            using (var tx = session.BeginTransaction())
            {
                await SeedRolesAndClaims(roleManager, session);
                await tx.CommitAsync();
            }
        }

        private static async Task SeedRolesAndClaims(RoleManager<Role> roleManager, ISession session)
        {
            if (!await roleManager.RoleExistsAsync(TokenExtensions.JwtClaims.ApiAccess))
            {
                await roleManager.CreateAsync(new Role
                {
                    Name = TokenExtensions.JwtClaims.ApiAccess
                });
            }
        }
    }
}
