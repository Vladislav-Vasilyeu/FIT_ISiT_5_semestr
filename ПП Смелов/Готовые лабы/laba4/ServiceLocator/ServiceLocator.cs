using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ServiceLocatorLibrary
{
    public class ServiceLocator
    {
        private readonly ConcurrentDictionary<Type, ServiceDescriptor> _services = new ConcurrentDictionary<Type, ServiceDescriptor>();
        private readonly ConcurrentDictionary<Type, object> _singletonServices = new ConcurrentDictionary<Type, object>();

        public ServiceLocator()
        {
            var scopeFactory = new ServiceScopeFactory(this);
            _singletonServices.TryAdd(typeof(ServiceScopeFactory), scopeFactory);
            _services.TryAdd(typeof(ServiceScopeFactory), new ServiceDescriptor(typeof(ServiceScopeFactory), typeof(ServiceScopeFactory), ServiceLifetime.Singleton, Array.Empty<object>()));
        }

        public void RegisterService(Type serviceType, Type implementationType, ServiceLifetime lifetime, params object[] parameters)
        {
            var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime, parameters);
            _services[serviceType] = descriptor;
        }

        public void RegisterInstance(Type serviceType, object instance)
        {
            var descriptor = new ServiceDescriptor(serviceType, serviceType, ServiceLifetime.Singleton, Array.Empty<object>());
            _singletonServices[serviceType] = instance;
            _services[serviceType] = descriptor;
        }

        public object GetService(Type serviceType, ServiceScope? scope = null)
        {
            if (!_services.TryGetValue(serviceType, out var descriptor))
            {
                throw new InvalidOperationException($"Service of type {serviceType} is not registered.");
            }

            

            switch (descriptor.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    return _singletonServices.GetOrAdd(serviceType, _ => CreateInstance(descriptor, scope));
                case ServiceLifetime.Scoped:
                    if (scope == null)
                        throw new InvalidOperationException("Scoped service requires an active scope.");
                    return scope.GetOrCreateScopedService(descriptor);
                case ServiceLifetime.Transient:
                    return CreateInstance(descriptor, scope);
                default:
                    throw new InvalidOperationException("Unknown service lifetime.");
            }
        }

        internal object CreateInstance(ServiceDescriptor descriptor, ServiceScope? scope)
        {
            var constructor = descriptor.ImplementationType.GetConstructors().FirstOrDefault();
            if (constructor == null)
                throw new InvalidOperationException($"No constructor found for type {descriptor.ImplementationType}");

            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];
            var availableParameters = descriptor.Parameters.ToList(); // Копируем параметры для отслеживания

            Console.WriteLine($"\nCreating instance of {descriptor.ImplementationType.Name}");
            Console.WriteLine($"Constructor parameters: {string.Join(", ", parameters.Select(p => p.ParameterType.Name))}");
            Console.WriteLine($"Provided parameters: {string.Join(", ", descriptor.Parameters.Select(p => p?.GetType()?.Name ?? "null"))}\n");

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                // Ищем подходящий параметр среди доступных по типу
                var providedParam = availableParameters.FirstOrDefault(p => p != null && paramType.IsAssignableFrom(p.GetType()));

                if (providedParam != null)
                {
                    Console.WriteLine($"Using provided parameter {i}: {providedParam.GetType().Name} for {paramType.Name}");
                    args[i] = providedParam;
                    // Удаляем использованный параметр из доступных
                    availableParameters.Remove(providedParam);
                }
                else
                {
                    Console.WriteLine($"Resolving parameter {i}: {paramType.Name} via GetService");
                    args[i] = GetService(paramType, scope)
                        ?? throw new InvalidOperationException($"Cannot resolve parameter {paramType.Name} for {descriptor.ImplementationType.Name}");
                }
            }

            try
            {
                return Activator.CreateInstance(descriptor.ImplementationType, args);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create instance of {descriptor.ImplementationType.Name}: {ex.Message}", ex);
            }
        }

        public ServiceScopeFactory GetScopeFactory()
        {
            return (ServiceScopeFactory)(GetService(typeof(ServiceScopeFactory))
                ?? throw new InvalidOperationException("ServiceScopeFactory not registered."));
        }
    }
}