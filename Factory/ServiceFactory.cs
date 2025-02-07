using ExtractCodeAPI.Services.Abstractions;
using ExtractCodeAPI.Services;
using System;

namespace ExtractCodeAPI.Factory
{
    public class ServiceFactory : IServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T CreateService<T>() where T : class
        {
            return _serviceProvider.GetService<T>() ?? throw new InvalidOperationException($"Serviciul {typeof(T).Name} nu a fost înregistrat.");
        }
    }
}
