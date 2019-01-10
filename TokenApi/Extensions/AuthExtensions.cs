using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace TokenApi.Extensions
{
    public static class AuthExtensions
    {
        public const string AdminClaim = "admin";
        public const string UserClaim = "user";
        public const string ManageUserClaim = "manage_user";
        public const string AdminRole = "admin";
        public const string UserRole = "user";

        public const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        public static ModelStateDictionary AddErrorsToModelState(this ModelStateDictionary modelState, IdentityResult identityResult)
        {
            foreach (var e in identityResult.Errors)
            {
                modelState.TryAddModelError(e.Code, e.Description);
            }

            return modelState;
        }
    }
}
