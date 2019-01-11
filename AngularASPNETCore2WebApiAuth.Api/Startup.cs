using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Dialect;
using NHibernate.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using nH.Identity.Extensions;
using nH.Identity.Mappings;
using AngularASPNETCore2WebApiAuth.Api.Auth;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using nH.Identity.Core;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using FluentValidation.AspNetCore;
using NHibernate.Tool.hbm2ddl;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using AngularASPNETCore2WebApiAuth.Api.ViewModels.Mappings;
using AngularASPNETCore2WebApiAuth.Api.Entities.Mappings;

namespace AngularASPNETCore2WebApiAuth.Api
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
            services.AddSingleton<IJwtFactory, JwtFactory>();

            services.AddScoped<IRefreshTokenFactory, RefreshTokenFactory>();

            services.AddHttpContextAccessor();

            // add identity (ORDER IS IMPORTANT AS FUCK)
            //services.AddIdentityCoreWithRole<User, Role>(o =>
            services.AddIdentityCore<User>(o =>
            {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            })
            .AddRoleToIdentity<Role>()
            .RegisterSessionStores()
            .AddDefaultTokenProviders();

            // jwt wire up
            // Get options from app settings
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));
            services.Configure<JwtIssuerOptions>(jwtAppSettingOptions);
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtAppSettingOptions["SecretKey"]));
            // Configure JwtIssuerOptions
            services.PostConfigure<JwtIssuerOptions>(options =>
            {
                options.SecurityKey = securityKey;
                options.SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.SaveToken = true;
#if DEBUG
                configureOptions.RequireHttpsMetadata = false;
#endif
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                    ValidateAudience = true,
                    ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,

                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // api user claim policy
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiUser", policy => policy.RequireClaim("rol", "API_ACCESS"));
            });

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

            services.AddAutoMapper();

            services.AddTransient<IStartupFilter, ProfileStartupFilter>();

            services.AddCors();
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CorsPolicy",
            //        builder => builder.AllowAnyOrigin()
            //          .AllowAnyMethod()
            //          .AllowAnyHeader()
            //          .AllowCredentials()
            //    .Build());
            //});

            services.AddRouting();

            services.AddMvcCore()
                    .AddFormatterMappings()
                    .AddJsonFormatters()
                    .AddAuthorization()//this also calls services.AddAuthorization();
                    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

        }

        public override void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler(
                builder =>
                {
                    builder.Run(
                        async context =>
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                                var error = context.Features.Get<IExceptionHandlerFeature>();
                                if (error != null)
                                {
                                    context.Response.Headers.Add("Application-Error", error.Error.Message);
                                    // CORS
                                    context.Response.Headers.Add("access-control-expose-headers", "Application-Error");
                                    await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                                }
                            });
                });

            app.UseAuthentication();

            app.UseCors(opt => opt.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials());
            //app.UseCors("CorsPolicy");

            app.UseMvc();
        }
    }
}
