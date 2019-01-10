using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using nH.Identity.Core;
using nH.Identity.Impl;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nH.Identity.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IdentityBuilder AddRoleToIdentity<TRole>(this IdentityBuilder builder) where TRole: class
        {
            builder = new IdentityBuilder(builder.UserType, typeof(TRole), builder.Services);
            builder.Services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
            var userClaimsPrincipalFactoryServiceType = typeof(IUserClaimsPrincipalFactory<>).MakeGenericType(builder.UserType);
            builder.Services.RemoveAll(userClaimsPrincipalFactoryServiceType);
            builder.Services.TryAddScoped(userClaimsPrincipalFactoryServiceType, typeof(UserClaimsPrincipalFactory<,>).MakeGenericType(builder.UserType, builder.RoleType));
            builder.Services.TryAddScoped<RoleManager<TRole>>();
            return builder;
        }
        /// <summary>
        /// Adds the default identity system configuration for the specified User and Role types. (Without Authentication Scheme)
        /// </summary>
        /// <typeparam name="TUser">The type representing a User in the system.</typeparam>
        /// <typeparam name="TRole">The type representing a Role in the system.</typeparam>
        /// <param name="services">The services available in the application.</param>
        /// <returns>An <see cref="IdentityBuilder"/> for creating and configuring the identity system.</returns>
        public static IdentityBuilder AddIdentityCoreWithRole<TUser, TRole>(this IServiceCollection services)
            where TUser : class
            where TRole : class
            => services.AddIdentityCoreWithRole<TUser, TRole>(setupAction: null);

        /// <summary>
        /// Adds and configures the identity system for the specified User and Role types. (Without Authentication Scheme)
        /// </summary>
        /// <typeparam name="TUser">The type representing a User in the system.</typeparam>
        /// <typeparam name="TRole">The type representing a Role in the system.</typeparam>
        /// <param name="services">The services available in the application.</param>
        /// <param name="setupAction">An action to configure the <see cref="IdentityOptions"/>.</param>
        /// <returns>An <see cref="IdentityBuilder"/> for creating and configuring the identity system.</returns>
        public static IdentityBuilder AddIdentityCoreWithRole<TUser, TRole>(this IServiceCollection services, Action<IdentityOptions> setupAction)
            where TUser : class
            where TRole : class
        {
            // Hosting doesn't add IHttpContextAccessor by default
            services.AddHttpContextAccessor();
            // Identity services
            services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
            services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
            services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
            // No interface for the error describer so we can add errors without rev'ing the interface
            services.TryAddScoped<IdentityErrorDescriber>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
            services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<TUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser, TRole>>();
            services.TryAddScoped<UserManager<TUser>>();
            services.TryAddScoped<SignInManager<TUser>>();
            services.TryAddScoped<RoleManager<TRole>>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return new IdentityBuilder(typeof(TUser), typeof(TRole), services);
        }

        public static IdentityBuilder RegisterSessionStores(this IdentityBuilder builder)
        {
            if(builder.UserType != typeof(User))
            {
                throw new NotSupportedException("RoleType must be of nH.Identity.User, nH.Identity type");
            }
            builder.Services.AddScoped(typeof(IUserStore<User>), typeof(UserStore));
            if (builder.RoleType != null)
            {
                if(builder.RoleType != typeof(Role))
                {
                    throw new NotSupportedException("RoleType must be of nH.Identity.Role, nH.Identity type");
                }
                builder.Services.AddScoped(typeof(IRoleStore<Role>), typeof(RoleStore));
            }
            return builder;
        }

        public static IServiceCollection RegisterClassMappingsFromAssemblyOf<TClassMapping>(this IServiceCollection services)
        {
            var mappingTypes = (from t in typeof(TClassMapping).Assembly.GetTypes()
                                where t.BaseType != null && t.BaseType.IsGenericType
                                where typeof(IConformistHoldersProvider).IsAssignableFrom(t)
                                select t);
            foreach (var mappingType in mappingTypes)
            {
                services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IConformistHoldersProvider), mappingType));
            }
            return services;
        }

        public static IServiceCollection ConfigurePersistence<TDialect, TDriver>(this IServiceCollection services, string connectionStringName, Action<Configuration> onConfigurationCreated = null)
            where TDialect : Dialect
            where TDriver : IDriver
        {
            services.AddSingleton((ctx) =>
            {
                var mapper = new ModelMapper();
                var mappings = ctx.GetServices<IConformistHoldersProvider>();
                foreach(var mapping in mappings)
                {
                    mapper.AddMapping(mapping);
                }
                return mapper;
            });
            services.AddSingleton((ctx) =>
            {
                var configuration = ctx.GetService<IConfiguration>();
                var nhConfig = new Configuration()
                .DataBaseIntegration(db =>
                {
                    //db.Dialect<Oracle10gDialect>();
                    //db.Dialect<MySQL57Dialect>();
                    db.Dialect<TDialect>();
                    //db.ConnectionString = configuration.GetSection("ConnectionStrings")["OracleDev"];
                    //db.ConnectionString = configuration.GetSection("ConnectionStrings")["MySQLDev"];
                    db.ConnectionString = configuration.GetConnectionString(connectionStringName);
                    //db.Driver<OracleManagedDriver>();
                    //db.Driver<MySqlDataDriver>();
                    db.Driver<TDriver>();
                    db.BatchSize = 100;
                });
                var mapper = ctx.GetRequiredService<ModelMapper>();
                var hbm = mapper.CompileMappingForAllExplicitlyAddedEntities();
                nhConfig.AddMapping(hbm);
                onConfigurationCreated?.Invoke(nhConfig);
                return nhConfig;
            });
            services.AddSingleton((ctx) => ctx.GetRequiredService<Configuration>().BuildSessionFactory());
            services.AddScoped((ctx) => ctx.GetRequiredService<ISessionFactory>().OpenSession());
            services.AddScoped((ctx) => ctx.GetRequiredService<ISessionFactory>().OpenStatelessSession());
            return services;
        }
    }
}
