using System;
using ServiceLocator;

namespace Test_ServiceLocator
{
    // Интерфейсы для тестирования разных жизненных циклов
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

    // Реализация Transient сервиса
    public class TransientService : ITransientService
    {
        private readonly Guid _id = Guid.NewGuid();
        public void Print() => Console.WriteLine($"Transient Service, ID: {_id}");
    }

    // Реализация Scoped сервиса
    public class ScopedService : IScopedService
    {
        private readonly Guid _id = Guid.NewGuid();
        public void Print() => Console.WriteLine($"Scoped Service, ID: {_id}");
    }

    // Реализация Singleton сервиса
    public class SingletonService : ISingletonService
    {
        private readonly Guid _id = Guid.NewGuid();
        public void Print() => Console.WriteLine($"Singleton Service, ID: {_id}");
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Создаём ServiceLocator
            var locator = new Locator();

            // Регистрируем сервисы
            locator.RegisterService(typeof(ITransientService), typeof(TransientService), ServiceLifetime.Transient);
            locator.RegisterService(typeof(IScopedService), typeof(ScopedService), ServiceLifetime.Scoped);
            locator.RegisterService(typeof(ISingletonService), typeof(SingletonService), ServiceLifetime.Singleton);

            // Тестируем Singleton
            Console.WriteLine("Testing Singleton:");
            var singleton1 = (ISingletonService)locator.GetService(typeof(ISingletonService));
            var singleton2 = (ISingletonService)locator.GetService(typeof(ISingletonService));
            singleton1.Print();
            singleton2.Print(); // Одинаковый ID

            // Тестируем Transient
            Console.WriteLine("\nTesting Transient:");
            var transient1 = (ITransientService)locator.GetService(typeof(ITransientService));
            var transient2 = (ITransientService)locator.GetService(typeof(ITransientService));
            transient1.Print();
            transient2.Print(); // Разные ID

            // Тестируем Scoped
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