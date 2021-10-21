using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AOS.Client.Models;
using AOS.Common.Constants;
using AOS.Common.DataSerialization;
using AOS.Common.Extensions;
using AOS.Common.Models;
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
                    Name = "Serialization example",
                    Description = string.Empty,
                    Action = SerializationExample
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

        private async Task SerializationExample()
        {
            var obj = new SerializableObject
            {
                Num = 1024 * 1024,
                SmallNum = 128,
                Text = "Main object",
                Objects = new List<SerializableSubObject>
                {
                    new SerializableSubObject
                    {
                        AnotherNum = 1,
                        AnotherText = "Child 1",
                        Child = new SerializableNestedObject
                        {
                            NestedNum = 11,
                            Strings = "Hello world".Split(' ').ToList()
                        }
                    },
                    new SerializableSubObject
                    {
                        AnotherNum = 2,
                        AnotherText = "Child 2",
                        Child = new SerializableNestedObject
                        {
                            NestedNum = 21,
                            Strings = "This is array of strings".Split(' ').ToList()
                        }
                    },
                    new SerializableSubObject
                    {
                        AnotherNum = 3,
                        AnotherText = "Child 3",
                        Child = new SerializableNestedObject
                        {
                            NestedNum = 11,
                            Strings = new List<string>()
                        }
                    }
                }
            };

            using var connection = await CreateConnectionAsync();

            await connection.Socket.SendMessageAsync(Headers.SerializationExample, obj);

            var response = await connection.Socket.ReceiveMessageAsync();

            if (response.Header != Headers.SerializationExample)
            {
                Console.WriteLine("Unexpected error occurred");
                Console.ReadKey();
                return;
            }

            var receivedObject = SConverter.Deserialize<SerializableObject>(response.BodyBytes);

            var json = JsonSerializer.Serialize(receivedObject, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            Console.WriteLine(json);
            Console.ReadKey();
        }

        private Task Exit()
        {
            Stop();
            return Task.CompletedTask;
        }
    }
}
