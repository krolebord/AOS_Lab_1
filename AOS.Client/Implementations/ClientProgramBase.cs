using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AOS.Client.Models;
using AOS.Client.Utils;
using AOS.Common.Interfaces;
using AOS.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AOS.Client.Implementations
{
    public abstract class ClientProgramBase : IProgram
    {
        public ProgramContext Context { get; set; } = default!;

        private bool _isRunning = true;

        private readonly ItemSelector<ConsoleAction> _selector;

        private readonly ConnectionFactory _connectionFactory;

        protected ClientProgramBase(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;

            // ReSharper disable once VirtualMemberCallInConstructor
            _selector = new ItemSelector<ConsoleAction>(BuildActions(), -1);
        }

        protected abstract List<ConsoleAction> BuildActions();

        public async Task RunAsync()
        {
            while (_isRunning)
            {
                DrawMenu();

                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                        _selector.SelectNext();
                        break;
                    case ConsoleKey.UpArrow:
                        _selector.SelectPrevious();
                        break;
                    case ConsoleKey.Spacebar:
                        Console.Clear();

                        await PerformAction(_selector.GetSelected());
                        break;

                }
            }
        }

        private void DrawMenu()
        {
            Console.Clear();

            Console.WriteLine("Available actions:");

            for (int i = 0; i < _selector.Items.Count; i++)
            {
                var action = _selector.Items[i];
                var label = $"{i+1}. {action.Name}";

                if (_selector.SelectedIndex != -1 && _selector.IsSelected(action))
                {
                    ConsoleUtils.WriteColoredLine(label, ConsoleColor.Black, ConsoleColor.White);
                }
                else
                {
                    Console.WriteLine(label);
                }
            }

            Console.WriteLine();
            if (_selector.SelectedIndex == -1)
            {
                Console.WriteLine("Controls:");
                Console.WriteLine("\tMove: arrow up/down");
                Console.WriteLine("\tConfirm: space");
            }
            else
            {
                Console.WriteLine(_selector.GetSelected().Description);
            }
        }

        public async Task PerformAction(ConsoleAction action)
        {
            try
            {
                await action.Action();
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("Couldn't perform action: " + action.Name);
                Console.WriteLine("Reason: " + e);
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }
        }

        protected void Stop() => _isRunning = false;

        protected Task<ServerConnection> CreateConnectionAsync()
        {
            var logger = Context.ServiceProvider.GetRequiredService<ILogger<ServerConnection>>();
            return _connectionFactory.CreateConnectionAsync(logger);
        }
    }
}
