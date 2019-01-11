using AngularASPNETCore2WebApiAuth.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using nH.Identity.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AngularASPNETCore2WebApiAuth.Api.Auth
{
    public class DynamicRolePolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly AuthorizationOptions _options;
        private readonly RoleManager<Role> _roleManager;

        public DynamicRolePolicyProvider(IOptions<AuthorizationOptions> options, RoleManager<Role> roleManager) : base(options)
        {
            this._options = options.Value;
            this._roleManager = roleManager;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // Check static policies first
            var policy = await base.GetPolicyAsync(policyName);

            if (policy == null)
            {
                policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new HasRoleRequirement(_roleManager.NormalizeKey(policyName)))
                    .Build();

                // Add policy to the AuthorizationOptions, so we don't have to re-create it each time
                // BUT is this thread safe?
                _options.AddPolicy(policyName, policy);
            }

            return policy;
        }
    }
}
