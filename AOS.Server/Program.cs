using System;
using System.Threading.Tasks;
using AOS.Common.DependencyInjection;
using AOS.Common.Implementations;
using AOS.Server.Implementations;
using AOS.Server.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AOS.Server
{
    class Program
    {
        static async Task Main()
        {
            Console.Title = "AOS Lab1 Server";

            var program = new ProgramBuilder<ServerProgram>()
                .SetConfiguration(builder => builder.AddDefaultConfiguration())
                .ConfigureServices(services => services
                    .AddLogging(builder => builder.AddConsole())
                    .AddTransient<IClientHandler, ClientHandler>())
                .Build();

            await program.RunAsync();
        }
    }
}
