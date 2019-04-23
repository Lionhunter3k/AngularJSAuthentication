using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASOS.Identity.Api.Services
{
    public class ClientApplicationCorsPolicyProvider : ICorsPolicyProvider
    {
        private readonly CorsOptions _options;

        public ClientApplicationCorsPolicyProvider(IOptions<CorsOptions> options)
        {
            _options = options.Value;
        }

        public async Task<CorsPolicy> GetPolicyAsync(HttpContext context, string policyName)
        {
            var clientApplicationStore = context.RequestServices.GetRequiredService<IClientApplicationStore>();
            var allowedRedirectUris = await clientApplicationStore.GetAllAllowedRedirectUrisAsync();
            var defaultPolicy = _options.GetPolicy(policyName ?? _options.DefaultPolicyName);
            var builder = new CorsPolicyBuilder(defaultPolicy);
            if (!defaultPolicy.AllowAnyOrigin)
            {
                builder = builder.WithOrigins(allowedRedirectUris.Select(r => new Uri(r).GetLeftPart(UriPartial.Authority)).ToArray());
            }
            return builder.AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials()
                     .Build();
        }
    }
}
