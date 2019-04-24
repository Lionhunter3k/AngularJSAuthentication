using ASOS.Identity.Api.Entities.Mappings;
using ASOS.Identity.Api.Providers;
using ASOS.Identity.Api.Services;
using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Server;
using Authentication.Api.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;
using nH.Identity.Core;
using nH.Identity.Extensions;
using nH.Identity.Mappings;
using nH.Infrastructure.Extensions;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;
using System;
using System.IO;
using System.Threading.Tasks;

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
                .UseMappingsFromAssemblyOf<ClientApplicationMap>()
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

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = OAuthValidationDefaults.AuthenticationScheme;
            })
               .AddOpenIdConnectServer(options =>
               {
                    // Create your own authorization provider by subclassing
                    // the OpenIdConnectServerProvider base class.
                    options.Provider = new AuthorizationProvider();
                    // Enable the authorization and token endpoints.
                    options.AuthorizationEndpointPath = "/connect/authorize";
                    options.TokenEndpointPath = "/token";
                    // During development, you can set AllowInsecureHttp
                    // to true to disable the HTTPS requirement.
                    options.AllowInsecureHttp = HostingEnvironment.IsDevelopment();

                    // Note: uncomment this line to issue JWT tokens.
                    // options.AccessTokenHandler = new JwtSecurityTokenHandler();
                }).AddOAuthValidation(options =>
                {
                    options.Audiences.Add("resource_server");
                });


            services.AddAuthorization(options =>
            {
                options.AddPolicy("Api_Access", policy => policy.RequireClaim("ASOS_Claim"));
            });

            services.AddCors(options =>
            {
                if (HostingEnvironment.IsDevelopment())
                {
                    options.AddDefaultPolicy(corsBuilder =>
                    {
                        corsBuilder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin()
                        .AllowCredentials();
                    });
                }
            });
            services.AddTransient<ICorsPolicyProvider, ClientApplicationCorsPolicyProvider>();
            services.AddScoped<IClientApplicationStore, ClientApplicationStore>();

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
            app.Use(async (context, next) =>
            {
                if (context.Request.Query.TryGetValue("bearer", out var urlBearer))
                {
                    context.Request.Headers.Add("Authorization", "Bearer " + urlBearer);
                }
                else if(context.Request.Cookies.TryGetValue("bearer", out var cookieBearer))
                {
                    context.Request.Headers.Add("Authorization", "Bearer " + cookieBearer);
                }
                await next();
            });
            app.UseCors();
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
