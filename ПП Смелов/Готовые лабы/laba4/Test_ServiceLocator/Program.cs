using System;
using ServiceLocatorLibrary;

namespace Test_ServiceLocator
{
    public interface ITransientService
    {
        void Print();
    }

    public interface IScopedService
    {
        void Print();
    }

    public interface ISingletonService
    {
        void Print();
    }

    public class TransientService : ITransientService
    {
        private readonly Guid _id = Guid.NewGuid();
        public void Print() => Console.WriteLine($"Transient Service, ID: {_id}");
    }

    public class ScopedService : IScopedService
    {
        private readonly Guid _id = Guid.NewGuid();
        public void Print() => Console.WriteLine($"Scoped Service, ID: {_id}");
    }

    public class SingletonService : ISingletonService
    {
        private readonly Guid _id = Guid.NewGuid();
        public void Print() => Console.WriteLine($"Singleton Service, ID: {_id}");
    }

    class Program
    {
        static void Main(string[] args)
        {
            
            var locator = new ServiceLocator();

            
            locator.RegisterService(typeof(ITransientService), typeof(TransientService), ServiceLifetime.Transient);
            locator.RegisterService(typeof(IScopedService), typeof(ScopedService), ServiceLifetime.Scoped);
            locator.RegisterService(typeof(ISingletonService), typeof(SingletonService), ServiceLifetime.Singleton);

            
            Console.WriteLine("Testing Singleton:");
            var singleton1 = (ISingletonService)locator.GetService(typeof(ISingletonService));
            var singleton2 = (ISingletonService)locator.GetService(typeof(ISingletonService));
            singleton1.Print();
            singleton2.Print(); // Одинаковый ID

            
            Console.WriteLine("\nTesting Transient:");
            var transient1 = (ITransientService)locator.GetService(typeof(ITransientService));
            var transient2 = (ITransientService)locator.GetService(typeof(ITransientService));
            transient1.Print();
            transient2.Print(); // Разные ID

            
            Console.WriteLine("\nTesting Scoped:");
            var scopeFactory = locator.GetScopeFactory();
            using (var scope1 = scopeFactory.CreateScope())
            {
                var scoped1 = (IScopedService)scope1.GetService(typeof(IScopedService));
                var scoped2 = (IScopedService)scope1.GetService(typeof(IScopedService));
                scoped1.Print();
                scoped2.Print(); // Одинаковый ID в пределах scope
            }

            using (var scope2 = scopeFactory.CreateScope())
            {
                var scoped3 = (IScopedService)scope2.GetService(typeof(IScopedService));
                scoped3.Print(); // Новый ID для нового scope
            }
        }
    }
}