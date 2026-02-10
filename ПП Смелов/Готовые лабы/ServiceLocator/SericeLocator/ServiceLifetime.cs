using System;

namespace ServiceLocatorLibrary
{
    public enum ServiceLifetime
    {
        Singleton,
        Scoped,
        Transient
    }
}