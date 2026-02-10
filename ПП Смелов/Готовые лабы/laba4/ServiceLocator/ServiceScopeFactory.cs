using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLocatorLibrary
{
    public class ServiceScopeFactory
    {
        private readonly ServiceLocator _serviceLocator;

        public ServiceScopeFactory(ServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public ServiceScope CreateScope()
        {
            return new ServiceScope(_serviceLocator);
        }
    }
}
