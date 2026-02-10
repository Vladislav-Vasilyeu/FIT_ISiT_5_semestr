using System;
using ServiceLocatorLibrary;

namespace Test_ServiceLocator
{
    public interface ITestService
    {
        Guid Id { get; }
        void PrintId();
    }

    public class TestService : ITestService
    {
        public Guid Id { get; } = Guid.NewGuid();

        public void PrintId()
        {
            Console.WriteLine($"ID сервиса: {Id}");
        }
    }

    class Program
    {
        static void Main()
        {
            var serviceLocator = new ServiceLocator();

            
            serviceLocator.RegisterService<ITestService>(() => new TestService(), ServiceLifetime.Transient);
            serviceLocator.RegisterService<ITestService>(() => new TestService(), ServiceLifetime.Scoped);
            serviceLocator.RegisterService<ITestService>(() => new TestService(), ServiceLifetime.Singleton);

            Console.WriteLine("=== Тестирование Singleton ===");
            var singleton1 = serviceLocator.ResolveSingleton<ITestService>();
            var singleton2 = serviceLocator.ResolveSingleton<ITestService>();
            singleton1.PrintId();
            singleton2.PrintId(); 

            Console.WriteLine("\n=== Тестирование Scoped ===");
            using var scope1 = serviceLocator.CreateScope();
            using var scope2 = serviceLocator.CreateScope();
            var scoped1 = scope1.ResolveScoped<ITestService>();
            var scoped2 = scope2.ResolveScoped<ITestService>();
            var scoped1Again = scope1.ResolveScoped<ITestService>();
            scoped1.PrintId(); 
            scoped1Again.PrintId(); 
            scoped2.PrintId(); 

            Console.WriteLine("\n=== Тестирование Transient ===");
            var transient1 = serviceLocator.ResolveTransient<ITestService>();
            var transient2 = serviceLocator.ResolveTransient<ITestService>();
            transient1.PrintId(); 
            transient2.PrintId();

            Console.ReadLine();
        }
    }
}