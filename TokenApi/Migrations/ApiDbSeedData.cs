using Microsoft.AspNetCore.Identity;
using nH.Identity.Core;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenApi.Extensions;

namespace TokenApi.Migrations
{
    public static class ApiDbSeedData
    {
        public static async Task Seed(UserManager<User> userManager, RoleManager<Role> roleManager, ISession session)
        {
            using (var tx = session.BeginTransaction())
            {
                await SeedRolesAndClaims(userManager, roleManager, session);
                await SeedAdmin(userManager, session);
                await tx.CommitAsync();
            }
        }

        private static async Task SeedRolesAndClaims(UserManager<User> userManager, RoleManager<Role> roleManager, ISession session)
        {

            if (!await roleManager.RoleExistsAsync(AuthExtensions.AdminRole))
            {
                await roleManager.CreateAsync(new Role
                {
                    Name = AuthExtensions.AdminRole
                });
            }

            if (!await roleManager.RoleExistsAsync(AuthExtensions.UserRole))
            {
                await roleManager.CreateAsync(new Role
                {
                    Name = AuthExtensions.UserRole
                });
            }

            var adminRole = await roleManager.FindByNameAsync(AuthExtensions.AdminRole);
            var adminRoleClaims = await roleManager.GetClaimsAsync(adminRole);

            if (!adminRoleClaims.Any(x => x.Type == AuthExtensions.ManageUserClaim))
            {
                await roleManager.AddClaimAsync(adminRole, new System.Security.Claims.Claim(AuthExtensions.ManageUserClaim, "true"));
            }
            if (!adminRoleClaims.Any(x => x.Type == AuthExtensions.AdminClaim))
            {
                await roleManager.AddClaimAsync(adminRole, new System.Security.Claims.Claim(AuthExtensions.AdminClaim, "true"));
            }

            var userRole = await roleManager.FindByNameAsync(AuthExtensions.UserRole);
            var userRoleClaims = await roleManager.GetClaimsAsync(userRole);
            if (!userRoleClaims.Any(x => x.Type == AuthExtensions.UserClaim))
            {
                await roleManager.AddClaimAsync(userRole, new System.Security.Claims.Claim(AuthExtensions.UserClaim, "true"));
            }
        }

        private static async Task SeedAdmin(UserManager<User> userManager, ISession session)
        {
            var u = await userManager.FindByNameAsync("admin");
            if (u == null)
            {
                u = new User
                {
                    DisplayName = "admin",
                    Email = "admin@nothing.com",
                    PhoneNumber = "0213213123",
                    SecurityStamp = Guid.NewGuid().ToString(),
                 };
                var x = await userManager.CreateAsync(u, "Admin1234!");
            }
            var uc = await userManager.GetClaimsAsync(u);
            if (!uc.Any(x => x.Type == AuthExtensions.AdminClaim))
            {
                await userManager.AddClaimAsync(u, new System.Security.Claims.Claim(AuthExtensions.AdminClaim, true.ToString()));
            }
            if (!await userManager.IsInRoleAsync(u, AuthExtensions.AdminRole))
                await userManager.AddToRoleAsync(u, AuthExtensions.AdminRole);
        }
    }
}
