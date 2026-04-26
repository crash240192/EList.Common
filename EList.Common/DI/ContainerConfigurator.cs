using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace EList.Common.DI
{
    public static class ContainerConfigurator
    {
        private static bool containerConfigured;
        private static readonly object syncRoot = new object();

        public static void Configure(IServiceCollection services, IServiceMappingProvider serviceMappingProvider)
        {
            if (!containerConfigured)
            {
                lock (syncRoot)
                {
                    if (!containerConfigured)
                    {
                        var mapping = serviceMappingProvider.GetServiceMapping();

                        foreach (var entry in mapping.Singletones)
                            services.AddSingleton(entry.Service, entry.Implementation);

                        foreach (var entry in mapping.Scopeds)
                            services.AddScoped(entry.Service, entry.Implementation);

                        foreach (var entry in mapping.Transients)
                            services.AddTransient(entry.Service, entry.Implementation);

                        containerConfigured = true;
                    }
                }
            }
        }

        public static void Configure(Container container, IServiceMappingProvider serviceMappingProvider)
        {
            if (!containerConfigured)
            {
                lock (syncRoot)
                {
                    if (!containerConfigured)
                    {
                        var mapping = serviceMappingProvider.GetServiceMapping();

                        foreach (var entry in mapping.Singletones)
                            container.Register(entry.Service, entry.Implementation, Lifestyle.Singleton);

                        //todo scoped to singleton for tests
                        foreach (var entry in mapping.Scopeds)
                            container.Register(entry.Service, entry.Implementation, Lifestyle.Singleton);

                        foreach (var entry in mapping.Transients)
                            container.Register(entry.Service, entry.Implementation, Lifestyle.Transient);

                        containerConfigured = true;
                    }
                }
            }
        }
    }
}