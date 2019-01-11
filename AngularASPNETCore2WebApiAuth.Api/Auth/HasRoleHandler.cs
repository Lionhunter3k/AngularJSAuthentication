using AngularASPNETCore2WebApiAuth.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using nH.Identity.Core;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AngularASPNETCore2WebApiAuth.Api.Auth
{
    public class HasRoleHandler : AuthorizationHandler<HasRoleRequirement>
    {
        private readonly UserManager<User> _userManager;

        public HasRoleHandler(UserManager<User> userManager)
        {
            this._userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasRoleRequirement requirement)
        {
            var userId = context.User.FindFirstValue(TokenExtensions.JwtClaimIdentifiers.Id);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (await _userManager.IsInRoleAsync(user, requirement.Role))
                    context.Succeed(requirement);
            }
        }
    }
}
