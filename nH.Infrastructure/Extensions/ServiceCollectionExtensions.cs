using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using nH.Infrastructure.Container;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace nH.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static NHibernateBuilder ConfigurePersistence(this IServiceCollection services)
        {
            return new NHibernateBuilder(services);
        }

        public static NHibernateBuilder UseConfigurationFile(this NHibernateBuilder nHibernateBuilder, string xmlCfgFileName = "hibernate.cfg.xml")
        {
            nHibernateBuilder.OnConfigurationCreated += (builder, configurationContext) => 
            {
                var hostingEnviroment = configurationContext.ServiceProvider.GetRequiredService<IHostingEnvironment>();
                var xmlConfigurationFilePath = Path.Combine(hostingEnviroment.ContentRootPath, xmlCfgFileName);
                configurationContext.Configuration.Configure(xmlConfigurationFilePath);
            };
            return nHibernateBuilder;
        }

        public static NHibernateBuilder SetupConfiguration(this NHibernateBuilder nHibernateBuilder, Action<IServiceProvider, Configuration> options)
        {
            nHibernateBuilder.OnConfigurationCreated += (builder, context) => options(context.ServiceProvider, context.Configuration);
            return nHibernateBuilder;
        }

        public static NHibernateBuilder UseDefaultModelMapper(this NHibernateBuilder nHibernateBuilder)
        {
            nHibernateBuilder.ServiceCollection.AddSingleton(ctx =>
            {
                var mapper = new ModelMapper();
                var mappings = ctx.GetServices<IConformistHoldersProvider>();
                foreach (var mapping in mappings)
                {
                    mapper.AddMapping(mapping);
                }
                return mapper.CompileMappingForAllExplicitlyAddedEntities();
            });
            return nHibernateBuilder;
        }

        public static NHibernateBuilder UseMappingsFromAssemblyOf<TClassMapping>(this NHibernateBuilder nHibernateBuilder)
        {
            var asssembly = typeof(TClassMapping).Assembly;
            if (nHibernateBuilder.Assemblies.Add(asssembly))
            {
                var mappingTypes = (from t in asssembly.GetTypes()
                                    where t.BaseType != null && t.BaseType.IsGenericType
                                    where typeof(IConformistHoldersProvider).IsAssignableFrom(t)
                                    select t);
                foreach (var mappingType in mappingTypes)
                {
                    nHibernateBuilder.ServiceCollection.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IConformistHoldersProvider), mappingType));
                }
            }
            return nHibernateBuilder;
        }

        public static NHibernateBuilder UseDatabaseIntegration<TDialect, TDriver>(this NHibernateBuilder nHibernateBuilder, string connectionStringName = "Default")
           where TDialect : Dialect
           where TDriver : IDriver
        {
            nHibernateBuilder.OnConfigurationCreated += (builder, configurationContext) =>
            {
                var configuration = configurationContext.ServiceProvider.GetService<IConfiguration>();
                var hostingEnviroment = configurationContext.ServiceProvider.GetRequiredService<IHostingEnvironment>();
                configurationContext.Configuration
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
            };
            return nHibernateBuilder;
        }
    }
}
