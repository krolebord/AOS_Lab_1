using System;
using System.Threading.Tasks;
using AOS.Client.Implementations;
using AOS.Common.DependencyInjection;
using AOS.Common.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace AOS.Client
{
    static class Program
    {
        static async Task Main()
        {
            Console.Title = "AOS Lab1 Server";

            var program = new ProgramBuilder<ClientProgram>()
                .SetConfiguration(builder => builder.AddDefaultConfiguration())
                .ConfigureServices(services => services
                    .AddLogging()
                    .AddSingleton<ConnectionFactory>())
                .Build();

            await program.RunAsync();
        }
    }
}
