using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            if (context.Request.Query.TryGetValue("client_id", out var clientId))
            {
                var clientApplication = await clientApplicationStore.GetClientApplicationAsync(clientId);
                if(clientApplication != null)
                {
                    return new CorsPolicyBuilder(clientApplication.AllowedRedirectUris.Select(r => new Uri(r).GetLeftPart(UriPartial.Authority)).ToArray())
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .Build();
                }
            }
            if(context.Request.HasFormContentType)
            {
                context.Request.EnableBuffering();
                var formData = await context.Request.ReadFormAsync();
                if(formData.TryGetValue("client_id", out clientId))
                {
                    var clientApplication = await clientApplicationStore.GetClientApplicationAsync(clientId);
                    if (clientApplication != null)
                    {
                        return new CorsPolicyBuilder(clientApplication.AllowedRedirectUris.Select(r => new Uri(r).GetLeftPart(UriPartial.Authority)).ToArray())
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .Build();
                    }
                }
            }
            return _options.GetPolicy(policyName ?? _options.DefaultPolicyName);
        }
    }
}
