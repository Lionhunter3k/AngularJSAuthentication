using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using nH.Identity.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using TokenApi.Auth;

namespace TokenApi.Middleware
{
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializerSettings;

        public TokenProviderMiddleware(
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
            try
            {
                var username = context.Request.Form["username"];
                var password = context.Request.Form["password"];

                var signInManager = context.RequestServices.GetService<SignInManager<User>>();
                var userManager = context.RequestServices.GetService<UserManager<User>>();
                var user = await userManager.FindByEmailAsync(username);
                var result = await signInManager.PasswordSignInAsync(user, password, false, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid username or password.");
                    return;
                }
                if (user.Deleted)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid username or password.");
                    return;
                }
                var getLokgToken = context.RequestServices.GetService<IJwtFactory>();
                var response = getLokgToken.GenerateEncodedToken(user);

                // Serialize and return the response
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
            }
            catch (Exception ex)
            {
                //TODO log error
                //Logging.GetLogger("Login").Error("Erorr logging in", ex);
            }
        }

    }

}
