using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AOS.Client.Models;
using AOS.Common.Constants;
using AOS.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AOS.Client.Implementations
{
    public class ClientProgram : ClientProgramBase
    {
        public ClientProgram(ConnectionFactory connectionFactory) : base(connectionFactory) { }

        protected override List<ConsoleAction> BuildActions()
        {
            return new()
            {
                new ConsoleAction()
                {
                    Name = "Who",
                    Description = "Show information about the author",
                    Action =  Who
                },
                new ConsoleAction()
                {
                    Name = "View files",
                    Description = "Start files viewing session",
                    Action = ViewFiles
                },
                new ConsoleAction()
                {
                    Name = "Exit",
                    Description = "Close application",
                    Action = Exit
                }
            };
        }

        private async Task Who()
        {
            using var connection = await CreateConnectionAsync();

            Console.WriteLine("Sending who command");

            await connection.Socket.SendMessageAsync(Headers.WhoCommand);

            var response = await connection.Socket.ReceiveMessageAsync();

            if (response.Header != Headers.WhoResponse)
            {
                Console.WriteLine("Received invalid response");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Response:");
            Console.WriteLine(response.GetBodyAs<string>());
            Console.ReadKey();
        }

        private async Task ViewFiles()
        {
            using var connection = await CreateConnectionAsync();

            Console.WriteLine("Successfully connected to server");

            var logger = Context.ServiceProvider.GetRequiredService<ILogger<ViewDirectoryProgram>>();
            using var program = new ViewDirectoryProgram(connection, logger);

            await program.Run();
        }

        private Task Exit()
        {
            Stop();
            return Task.CompletedTask;
        }
    }
}
