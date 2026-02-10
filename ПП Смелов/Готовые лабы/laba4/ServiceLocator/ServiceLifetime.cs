using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLocatorLibrary
{
    public enum ServiceLifetime
    {
        Transient,
        Scoped,
        Singleton
    }
}
