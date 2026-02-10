using System;

namespace ServiceLocatorLibrary
{
    public interface IServiceLocator : IDisposable
    {
        void RegisterService<TService>(Func<TService> factory, ServiceLifetime lifetime);
        TService ResolveSingleton<TService>();
        TService ResolveScoped<TService>();
        TService ResolveTransient<TService>();
        IServiceLocator CreateScope();
    }
}