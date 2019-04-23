using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OAuthTutorial.Data;
using OAuthTutorial.Models;
using OAuthTutorial.Services;
using nH.Identity.Extensions;
using nH.Identity.Core;
using NHibernate.Dialect;
using nH.Infrastructure.Extensions;
using nH.Identity.Mappings;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Driver;
using System.IO;
using OAuthTutorial.Entities.Mappings;

namespace OAuthTutorial
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add persistence
            services.ConfigurePersistence()
                .UseDatabaseIntegration<SQLiteDialect, SQLite20Driver>("SQLite")
                .UseDefaultModelMapper()
                .UseMappingsFromAssemblyOf<UserMap>()
                .UseMappingsFromAssemblyOf<TokenMap>()
                .SetupConfiguration((_, cfg) =>
                {
                    var schemaExport = new SchemaExport(cfg);
                    schemaExport
                    .SetOutputFile(Path.Combine(HostingEnvironment.ContentRootPath, "schema.sql"))
                    .Execute(true, true, false);
                });

            services.AddIdentity<User, Role>((x) => {
                x.Password.RequiredLength = 6;
                x.Password.RequiredUniqueChars = 0;
                x.Password.RequireNonAlphanumeric = false;
                x.Password.RequireDigit = false;
                x.Password.RequireLowercase = false;
                x.Password.RequireUppercase = false;
            })
              .RegisterSessionStores()
              .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
