using System;

namespace ServiceLocatorLibrary
{
    public class ServiceDescriptor
    {
        public Func<object> Factory { get; }
        public ServiceLifetime Lifetime { get; }

        public ServiceDescriptor(Func<object> factory, ServiceLifetime lifetime)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Lifetime = lifetime;
        }
    }
}