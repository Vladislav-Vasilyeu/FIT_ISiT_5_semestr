using System;
using System.Collections.Generic;

namespace ServiceLocatorLibrary
{
    public class ServiceLocator : IServiceLocator
    {
        private readonly Dictionary<(Type, ServiceLifetime), ServiceDescriptor> _services;
        private readonly Dictionary<Type, Lazy<object>> _singletonInstances; 
        private readonly Dictionary<Type, object> _scopedInstances;
        private readonly ServiceLocator _parentLocator;
        private bool _disposed;

        
        public ServiceLocator()
        {
            _services = new Dictionary<(Type, ServiceLifetime), ServiceDescriptor>();
            _singletonInstances = new Dictionary<Type, Lazy<object>>();
            _scopedInstances = new Dictionary<Type, object>();
        }

        
        private ServiceLocator(ServiceLocator parentLocator)
        {
            _parentLocator = parentLocator ?? throw new ArgumentNullException(nameof(parentLocator));
            _services = parentLocator._services; 
            _scopedInstances = new Dictionary<Type, object>();
            _disposed = false;
        }
        
        public void RegisterService<TService>(Func<TService> factory, ServiceLifetime lifetime)
        {
            if (_disposed)
                throw new ObjectDisposedException("Локатор сервисов освобождён");
            if (_parentLocator != null)
                throw new InvalidOperationException("Регистрация сервисов разрешена только в корневом локаторе");
            if (!typeof(TService).IsInterface && !typeof(TService).IsClass)
                throw new ArgumentException($"Тип сервиса {typeof(TService)} должен быть интерфейсом или классом");

            var key = (typeof(TService), lifetime);
            if (_services.ContainsKey(key))
                throw new InvalidOperationException($"Сервис типа {typeof(TService)} с жизненным циклом {lifetime} уже зарегистрирован");

            Func<object> objectFactory = () => factory() ?? throw new InvalidOperationException("Фабрика вернула null");
            _services[key] = new ServiceDescriptor(objectFactory, lifetime);
        }

        public TService ResolveSingleton<TService>()
        {
            return Resolve<TService>(ServiceLifetime.Singleton);
        }

        public TService ResolveScoped<TService>()
        {
            return Resolve<TService>(ServiceLifetime.Scoped);
        }

        public TService ResolveTransient<TService>()
        {
            return Resolve<TService>(ServiceLifetime.Transient);
        }

        private TService Resolve<TService>(ServiceLifetime lifetime)
        {
            if (_disposed)
                throw new ObjectDisposedException("Локатор сервисов освобождён");

            var serviceType = typeof(TService);
            var key = (serviceType, lifetime);
            if (!_services.TryGetValue(key, out var descriptor))
            {
                if (_parentLocator != null)
                    return _parentLocator.Resolve<TService>(lifetime);
                throw new InvalidOperationException($"Сервис типа {serviceType} с жизненным циклом {lifetime} не зарегистрирован");
            }

            return descriptor.Lifetime switch
            {
                ServiceLifetime.Singleton => GetSingletonInstance<TService>(descriptor),
                ServiceLifetime.Scoped => GetScopedInstance<TService>(descriptor),
                ServiceLifetime.Transient => (TService)descriptor.Factory(),
                _ => throw new InvalidOperationException("Неизвестный жизненный цикл сервиса")
            };
        }

        public IServiceLocator CreateScope()
        {
            if (_disposed)
                throw new ObjectDisposedException("Локатор сервисов освобождён");
            return new ServiceLocator(this);
        }

        private TService GetSingletonInstance<TService>(ServiceDescriptor descriptor)
        {
            
            if (_parentLocator != null)
                return _parentLocator.GetSingletonInstance<TService>(descriptor);

            var serviceType = typeof(TService);
            if (!_singletonInstances.TryGetValue(serviceType, out var lazyInstance))
            {
                lazyInstance = new Lazy<object>(descriptor.Factory, isThreadSafe: true);
                _singletonInstances[serviceType] = lazyInstance;
            }

            return (TService)lazyInstance.Value;
        }

        private TService GetScopedInstance<TService>(ServiceDescriptor descriptor)
        {
            var serviceType = typeof(TService);
            if (_scopedInstances.TryGetValue(serviceType, out var instance))
            {
                return (TService)instance;
            }

            instance = descriptor.Factory();
            _scopedInstances[serviceType] = instance;
            return (TService)instance;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _scopedInstances.Clear();
                if (_parentLocator == null)
                    _singletonInstances?.Clear(); 
                _disposed = true;
            }
        }
    }
}