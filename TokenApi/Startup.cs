using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nH.Identity.Mappings;
using NHibernate.Dialect;
using NHibernate.Driver;
using nH.Identity.Extensions;
using NHibernate.Tool.hbm2ddl;
using System.IO;
using nH.Identity.Core;
using Microsoft.AspNetCore.Identity;
using TokenApi.Extensions;
using System.Threading.Tasks;
using TokenApi.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using TokenApi.Middleware;
using TokenApi.Entities.Mappings;

namespace TokenApi
{
    public class Startup : StartupBase
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment Environment { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            // add persistence
            services.ConfigurePersistence<MsSql2012Dialect, SqlClientDriver>("SqlServer", cfg =>
            {
                var schemaExport = new SchemaExport(cfg);
                schemaExport
                .SetOutputFile(Path.Combine(Environment.ContentRootPath, "schema.sql"))
                .Execute(true, false, false);
            })
            .RegisterClassMappingsFromAssemblyOf<UserMap>()
            .RegisterClassMappingsFromAssemblyOf<RefreshTokenMap>();

            // add identity
            services.AddIdentityCoreWithRole<User, Role>(o =>
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

            // add cors
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .AllowCredentials();
                });
            });

            // add mvc
            services.AddMvcCore()
                  .AddFormatterMappings()
                  .AddJsonFormatters()
                  .AddAuthorization();//this also calls services.AddAuthorization();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("UserManagement", policy => policy.RequireClaim(AuthExtensions.ManageUserClaim));
                options.AddPolicy("Admin", policy => policy.RequireClaim(AuthExtensions.AdminClaim));
                options.AddPolicy("User", policy => policy.RequireClaim(AuthExtensions.UserClaim));
                options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole(AuthExtensions.AdminRole));
            });

            // security

            services.AddSingleton<IJwtFactory, JwtFactory>();

            // return 401 instead of redirect to login
            services.ConfigureApplicationCookie(options => {
                options.Events.OnRedirectToLogin = context => {
                    context.Response.Headers["Location"] = context.RedirectUri;
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });

            services.AddAuthentication()
                   .AddJwtBearer(cfg =>
                   {
                       cfg.RequireHttpsMetadata = false;
                          //cfg.SaveToken = true;

                          cfg.TokenValidationParameters = new TokenValidationParameters()
                       {
                           ValidateIssuer = true,
                           ValidateAudience = true,
                           ValidateLifetime = true,
                           ValidateIssuerSigningKey = true,
                           ValidIssuer = Configuration["TokenAuthentication:Issuer"],
                           ValidAudience = Configuration["TokenAuthentication:Audience"],
                           IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["TokenAuthentication:SecretKey"]))
                       };

                       cfg.Events = new JwtBearerEvents
                       {
                           OnAuthenticationFailed = context =>
                           {
                               Console.WriteLine("OnAuthenticationFailed: " +
                                   context.Exception.Message);
                               return Task.CompletedTask;
                           },
                           OnTokenValidated = context =>
                           {
                               Console.WriteLine("OnTokenValidated: " +
                                   context.SecurityToken);
                               return Task.CompletedTask;
                           }
                       };

                   });
        }

        public override void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");

            app.UseMiddleware<TokenProviderMiddleware>();

            app.UseMiddleware<RefreshTokenProviderMiddleware>();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}