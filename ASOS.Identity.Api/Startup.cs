using AspNet.Security.OpenIdConnect.Server;
using Authentication.Api.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nH.Identity.Core;
using nH.Identity.Extensions;
using nH.Identity.Mappings;
using nH.Infrastructure.Extensions;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;
using System.IO;

namespace ASOS.Identity.Api
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // add persistence
            services.ConfigurePersistence()
                .UseDatabaseIntegration<SQLiteDialect, SQLite20Driver>("SQLite")
                .UseDefaultModelMapper()
                .UseMappingsFromAssemblyOf<UserMap>()
                .SetupConfiguration((_, cfg) =>
                 {
                     var schemaExport = new SchemaExport(cfg);
                     schemaExport
                     .SetOutputFile(Path.Combine(HostingEnvironment.ContentRootPath, "schema.sql"))
                     .Execute(true, true, false);
                 });

            // add identity
            services.AddIdentityCore<User, Role>(o =>
            {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            })
            .RegisterSessionStores()
            .AddDefaultTokenProviders();

            services.AddAuthentication()
               .AddOpenIdConnectServer(options =>
               {
                    // Create your own authorization provider by subclassing
                    // the OpenIdConnectServerProvider base class.
                    options.Provider = new OpenIdConnectServerProvider();
                    // Enable the authorization and token endpoints.
                    options.AuthorizationEndpointPath = "/connect/authorize";
                   options.TokenEndpointPath = "/connect/token";
                    // During development, you can set AllowInsecureHttp
                    // to true to disable the HTTPS requirement.
                    options.AllowInsecureHttp = true;

                    // Note: uncomment this line to issue JWT tokens.
                    // options.AccessTokenHandler = new JwtSecurityTokenHandler();
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
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
