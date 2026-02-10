using System;
using System.Collections.Concurrent;

namespace ServiceLocatorLibrary
{
    public class ServiceScope : IDisposable
    {
        private readonly ConcurrentDictionary<Type, object> _scopedServices = new ConcurrentDictionary<Type, object>();
        private readonly ServiceLocator _serviceLocator;

        public ServiceScope(ServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public object? GetService(Type serviceType)
        {
            return _serviceLocator.GetService(serviceType, this);
        }

        public void Dispose()
        {
            foreach (var service in _scopedServices.Values)
            {
                if (service is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _scopedServices.Clear();
        }

        internal object GetOrCreateScopedService(ServiceDescriptor descriptor)
        {
            return _scopedServices.GetOrAdd(descriptor.ServiceType, _ => _serviceLocator.CreateInstance(descriptor, this));
        }
    }
}