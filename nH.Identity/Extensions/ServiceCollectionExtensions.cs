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
        public static IdentityBuilder RegisterSessionStores(this IdentityBuilder builder)
        {
            if(builder.UserType != typeof(User))
            {
                throw new NotSupportedException("RoleType must be of nH.Identity.User, nH.Identity type");
            }
            builder.Services.AddScoped(typeof(IUserStore<>), typeof(UserStore));
            if (builder.RoleType != null)
            {
                if(builder.RoleType != typeof(Role))
                {
                    throw new NotSupportedException("RoleType must be of nH.Identity.Role, nH.Identity type");
                }
                builder.Services.AddScoped(typeof(IRoleStore<>), typeof(RoleStore));
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
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IConformistHoldersProvider), mappingType));
            }
            return services;
        }

        public static IServiceCollection ConfigurePersistence<TDialect, TDriver>(this IServiceCollection services, string connectionStringName)
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
                return nhConfig;
            });
            services.AddSingleton((ctx) => ctx.GetRequiredService<Configuration>().BuildSessionFactory());
            services.AddScoped((ctx) => ctx.GetRequiredService<ISessionFactory>().OpenSession());
            services.AddScoped((ctx) => ctx.GetRequiredService<ISessionFactory>().OpenStatelessSession());
            return services;
        }
    }
}
