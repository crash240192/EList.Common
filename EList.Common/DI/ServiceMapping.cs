using System.Collections.Generic;

namespace EList.Common.DI
{
    public class ServiceMapping
    {
        private readonly List<ServiceMappingEntry> _singletones = new List<ServiceMappingEntry>();
        private readonly List<ServiceMappingEntry> _transients = new List<ServiceMappingEntry>();
        private readonly List<ServiceMappingEntry> _scopeds = new List<ServiceMappingEntry>();

        public IReadOnlyCollection<ServiceMappingEntry> Singletones => _singletones;
        public IReadOnlyCollection<ServiceMappingEntry> Transients => _transients;
        public IReadOnlyCollection<ServiceMappingEntry> Scopeds => _scopeds;

        public ServiceMapping AddSingleton<TImplementation>()
        {
            _singletones.Add(new ServiceMappingEntry
            {
                Service = typeof(TImplementation),
                Implementation = typeof(TImplementation)
            });

            return this;
        }

        public ServiceMapping AddSingleton<TService, TImplementation>()
            where TImplementation : TService
        {
            _singletones.Add(new ServiceMappingEntry
            {
                Service = typeof(TService),
                Implementation = typeof(TImplementation)
            });

            return this;
        }

        public ServiceMapping AddScoped<TImplementation>()
        {
            _scopeds.Add(new ServiceMappingEntry
            {
                Service = typeof(TImplementation),
                Implementation = typeof(TImplementation)
            });

            return this;
        }

        public ServiceMapping AddScoped<TService, TImplementation>()
            where TImplementation : TService
        {
            _scopeds.Add(new ServiceMappingEntry
            {
                Service = typeof(TService),
                Implementation = typeof(TImplementation)
            });

            return this;
        }

        public ServiceMapping AddTransient<TImplementation>()
        {
            _transients.Add(new ServiceMappingEntry
            {
                Service = typeof(TImplementation),
                Implementation = typeof(TImplementation)
            });

            return this;
        }

        public ServiceMapping AddTransient<TService, TImplementation>()
            where TImplementation : TService
        {
            _transients.Add(new ServiceMappingEntry
            {
                Service = typeof(TService),
                Implementation = typeof(TImplementation)
            });

            return this;
        }
    }
}