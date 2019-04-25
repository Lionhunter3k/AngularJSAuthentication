using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
using System;
using OAuthTutorial.Providers;
using Microsoft.AspNetCore.Authorization;
using OAuthTutorial.Policies;

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
                    .Execute(true, false, false);
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

            services.AddAuthentication()
                       .AddOAuthValidation()
                       .AddOpenIdConnectServer(options => {
                           options.UserinfoEndpointPath = "/api/v1/me";
                           options.TokenEndpointPath = "/api/v1/token";
                           options.AuthorizationEndpointPath = "/authorize/";
                           options.UseSlidingExpiration = false; // False means that new Refresh tokens aren't issued. Our implementation will be doing a no-expiry refresh, and this is one part of it.
                           options.AllowInsecureHttp = HostingEnvironment.IsDevelopment(); // ONLY FOR TESTING
                           options.AccessTokenLifetime = TimeSpan.FromHours(1); // An access token is valid for an hour - after that, a new one must be requested.
                           options.RefreshTokenLifetime = TimeSpan.FromDays(365 * 1000); //NOTE - Later versions of the ASOS library support `TimeSpan?` for these lifetime fields, meaning no expiration.
                                                                                          // The version we are using does not, so a long running expiration of one thousand years will suffice.
                           options.AuthorizationCodeLifetime = TimeSpan.FromSeconds(60);
                           options.IdentityTokenLifetime = options.AccessTokenLifetime;
                           options.ProviderType = typeof(OAuthProvider);
                        });

            // Add application services.
            services.AddTransient<IEmailSender, LocalFileEmailSender>();
            services.AddScoped<OAuthProvider>();
            services.AddScoped<ValidationService>();
            services.AddScoped<TokenService>();
            services.AddScoped<TicketCounter>();
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (HostingEnvironment.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
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
