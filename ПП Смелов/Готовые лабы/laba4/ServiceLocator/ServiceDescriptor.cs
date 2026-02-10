using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLocatorLibrary
{
    public class ServiceDescriptor
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public ServiceLifetime Lifetime { get; }
        public object? Implementation { get; set; }
        public object[] Parameters { get; }

        public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, params object[] parameters)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
            Parameters = parameters;
        }
    }
}
