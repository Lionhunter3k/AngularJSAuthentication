using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace nH.Infrastructure.Container
{
    public class NHibernateBuilder
    {
        public NHibernateBuilder(IServiceCollection serviceCollection)
        {
            this.ServiceCollection = serviceCollection
                             .AddSingleton(GetConfiguration)
                             .AddSingleton(GetSessionFactory)
                             .AddSingleton(GetModelMapper)
                             .AddScoped((ctx) => ctx.GetRequiredService<ISessionFactory>().OpenSession())
                             .AddScoped((ctx) => ctx.GetRequiredService<ISessionFactory>().OpenStatelessSession())
                             .AddScoped<StatefulSessionWrapper>()
                             .AddScoped<StatelessSessionWrapper>();
        }

        public IServiceCollection ServiceCollection { get; }

        public HashSet<Assembly> Assemblies { get; } = new HashSet<Assembly>();

        public event EventHandler<(IServiceProvider ServiceProvider, ISessionFactory SessionFactory)> OnSessionFactoryCreated;

        public event EventHandler<(IServiceProvider ServiceProvider, Configuration Configuration)> OnConfigurationCreated;

        private HbmMapping GetModelMapper(IServiceProvider ctx)
        {
            var mapper = new ModelMapper();
            var mappings = ctx.GetServices<IConformistHoldersProvider>();
            foreach (var mapping in mappings)
            {
                mapper.AddMapping(mapping);
            }
            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }

        private Configuration GetConfiguration(IServiceProvider context)
        {
            var config = new Configuration();
            foreach (var assembly in Assemblies)
            {
                config.AddAssembly(assembly);
            }
            if (Assemblies.Count > 0)
            {
                var hbmMapping = context.GetService<HbmMapping>();
                if (hbmMapping != null)
                {
                    config.AddMapping(hbmMapping);
                }
            }
            var onConfigurationCreated = OnConfigurationCreated;
            if (onConfigurationCreated != null)
            {
                onConfigurationCreated.Invoke(this, (context, config));
            }
            else
            {
                config.Configure();
            }
            return config;
        }

        private ISessionFactory GetSessionFactory(IServiceProvider context)
        {
            var sessionFactory = context.GetRequiredService<Configuration>().BuildSessionFactory();
            OnSessionFactoryCreated?.Invoke(this, (context, sessionFactory));
            return sessionFactory;
        }
    }
}
