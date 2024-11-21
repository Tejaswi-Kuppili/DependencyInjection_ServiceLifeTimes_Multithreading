using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DIWithMultithreading
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<ISingletonService, SingletonService>();
            serviceCollection.AddScoped<IScopedService, ScopedService>();
            serviceCollection.AddTransient<ITransientService, TransientService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (var scope1 = serviceProvider.CreateScope())
            {
                Console.WriteLine("Request 1:");
                var scoped1A = scope1.ServiceProvider.GetRequiredService<IScopedService>();
                var scoped1B = scope1.ServiceProvider.GetRequiredService<IScopedService>();
                scoped1A.PerformOperation();
                scoped1B.PerformOperation();

                var transient1A = scope1.ServiceProvider.GetRequiredService<ITransientService>();
                var transient1B = scope1.ServiceProvider.GetRequiredService<ITransientService>();
                transient1A.PerformOperation();
                transient1B.PerformOperation();
            }

            using (var scope2 = serviceProvider.CreateScope())
            {
                Console.WriteLine("Request 2:");
                var scoped2 = scope2.ServiceProvider.GetRequiredService<IScopedService>();
                scoped2.PerformOperation();
            }

            await DemonstrateSingletonMultithreading(serviceProvider);
            Console.ReadLine();
        }

        private static async Task DemonstrateSingletonMultithreading(IServiceProvider serviceProvider)
        {
            var singletonService = serviceProvider.GetRequiredService<ISingletonService>();
            Console.WriteLine();

            Console.WriteLine("Demonstrating Singleton Service with Multithreading:");
            var tasks = new Task[3];

            for (int i = 0; i < 3; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    singletonService.PerformOperation($"Task {i - 2}");
                });
            }

            await Task.WhenAll(tasks);
        }
    }

    public interface ISingletonService
    {
        void PerformOperation(string taskName);
    }

    public class SingletonService : ISingletonService
    {
        private readonly object _lock = new object();

        public void PerformOperation(string taskName)
        {
            lock (_lock)
            {
                Console.WriteLine($"\nSingleton Service Instance : {GetHashCode()}\n");
                Console.WriteLine($"{taskName}: Singleton Operation Started");
                Console.WriteLine($"{taskName}: Singleton Operation Finished");
            }
        }
    }

    public interface IScopedService
    {
        void PerformOperation();
    }

    public class ScopedService : IScopedService
    {
        public void PerformOperation()
        {
            Console.WriteLine($"Scoped Service Instance: {GetHashCode()}");
        }
    }

    public interface ITransientService
    {
        void PerformOperation();
    }

    public class TransientService : ITransientService
    {
        public void PerformOperation()
        {
            Console.WriteLine($"Transient Service Instance: {GetHashCode()}");
        }
    }
}
