using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using nH.Identity.Core;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TokenApi.Auth;
using TokenApi.Entities;

namespace TokenApi.Middleware
{
    public class RefreshTokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializerSettings;

        public RefreshTokenProviderMiddleware(
                    RequestDelegate next)
        {
            _next = next;

            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            // If the request path doesn't match, skip
            if (!context.Request.Path.Equals("/token", StringComparison.Ordinal))
            {
                return _next(context);
            }

            var grantType = context.Request.Form["grant_type"];
            if (grantType != "refresh_token")
            {
                return _next(context);
            }

            // Request must be POST with Content-Type: application/x-www-form-urlencoded
            if (!context.Request.Method.Equals("POST")
               || !context.Request.HasFormContentType)
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Bad request.");
            }

            return GenerateToken(context);
        }

        private async Task GenerateToken(HttpContext context)
        {
            var refreshToken = context.Request.Form["refresh_token"].ToString();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("User must relogin.");
                return;
            }
            var getLokgToken = context.RequestServices.GetService<IJwtFactory>();
            var signInManager = context.RequestServices.GetService<SignInManager<User>>();
            var userManager = context.RequestServices.GetService<UserManager<User>>();

            var user = await getLokgToken.GetUserAsync(refreshToken);

            if (user == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("User must relogin.");
                return;
            }

            if (!await signInManager.CanSignInAsync(user))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("User is unable to login.");
                return;
            }

            if (userManager.SupportsUserLockout && await userManager.IsLockedOutAsync(user))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("User is locked out.");
                return;
            }

            var token = await getLokgToken.GenerateEncodedTokenAsync(user, context.Request.Form["client_id"]);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(token, _serializerSettings));
        }
    }
}
