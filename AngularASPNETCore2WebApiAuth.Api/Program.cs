using AngularASPNETCore2WebApiAuth.Api.Migrations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nH.Identity.Core;
using NHibernate;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AngularASPNETCore2WebApiAuth.Api
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseDefaultServiceProvider((context, options) => options.ValidateScopes = context.HostingEnvironment.IsDevelopment())
            .UseStartup<Startup>()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json").AddUserSecrets("aspnet-IdentityDemo-7939A8EF-89B2-46F3-9E64-E33629F164CB").AddEnvironmentVariables();
                if (args == null)
                    return;
                config.AddCommandLine(args);
            })
            .UseUrls("https://localhost:26214")
            .Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var roleManager = services.GetRequiredService<RoleManager<Role>>();
                    var session = services.GetRequiredService<ISession>();
                    await ApiDbSeedData.Seed(roleManager, session);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            host.Run();
        }
    }
}
