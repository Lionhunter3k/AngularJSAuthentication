using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngularASPNETCore2WebApiAuth.Api.Extensions
{
    public static class ModelStateExtensions
    {
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
