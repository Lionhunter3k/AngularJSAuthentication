using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace AngularASPNETCore2WebApiAuth.Api
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseDefaultServiceProvider((context, options) => options.ValidateScopes = context.HostingEnvironment.IsDevelopment())
            .UseStartup<Startup>()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json").AddEnvironmentVariables();
                if (args == null)
                    return;
                config.AddCommandLine(args);
            })
            .UseUrls("http://localhost:26214")
            .Build();
            host.Run();
        }
    }
}
