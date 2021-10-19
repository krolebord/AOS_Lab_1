using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AOS.Client.Models;
using AOS.Client.Utils;
using AOS.Common.Constants;
using AOS.Common.Extensions;
using AOS.Common.Models.Data;
using AOS.Common.Models.Responses;
using Microsoft.Extensions.Logging;

namespace AOS.Client.Implementations
{
    public class ViewDirectoryProgram : IDisposable
    {
        private readonly ServerConnection _connection;
        private readonly ILogger<ViewDirectoryProgram> _logger;

        private bool _running = true;

        private DirectoryData? _currentDirectoryInfo;
        private readonly Stack<string> _pathStack = new();

        private ItemSelector<ConsoleAction> _selector = default!;

        public ViewDirectoryProgram(ServerConnection connection, ILogger<ViewDirectoryProgram> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task Run()
        {
            AddPathToStack(string.Empty);
            await UpdateDirectoryInfo();

            while (_running)
            {
                DrawMenu();

                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        _selector.SelectPrevious();
                        break;
                    case ConsoleKey.DownArrow:
                        _selector.SelectNext();
                        break;
                    case ConsoleKey.Spacebar:
                        await _selector.GetSelected().Action();
                        break;
                    case ConsoleKey.Escape:
                        Exit();
                        break;
                }
            }
        }

        private void DrawMenu()
        {
            Console.Clear();

            ConsoleUtils.WriteTopEdge();

            ConsoleUtils.WriteRow(BuildStackPath());

            ConsoleUtils.WriteMiddleEdge();

            foreach (var item in _selector.Items)
            {
                if (_selector.IsSelected(item))
                {
                    ConsoleUtils.WriteColoredRow(item.Name, ConsoleColor.Black, ConsoleColor.White);
                }
                else
                {
                    ConsoleUtils.WriteRow(item.Name);
                }
            }

            ConsoleUtils.WriteMiddleEdge();

            var text = _selector.GetSelected().Description.PadBoth(Console.WindowWidth - 2);

            ConsoleUtils.WriteColoredRow(text, ConsoleColor.Black, ConsoleColor.White);

            ConsoleUtils.WriteBottomEdge();
        }

        private void UpdateSelector(DirectoryData? info)
        {
            var actions = new List<ConsoleAction>();

            actions.Add(new ConsoleAction
            {
                Name = "..",
                Description = "Go back",
                Action = OpenPreviousPath
            });

            if (info is not null)
            {
                actions.AddRange(info.Directories.Select(x => new ConsoleAction
                {
                    Name = x + '/',
                    Description = "Open " + x,
                    Action = () => OpenPath(x)
                }));

                actions.AddRange(info.Files.Select(x => new ConsoleAction
                {
                    Name = x,
                    Description = "Read " + x,
                    Action = () => ReadFile(x)
                }));
            }

            actions.Add(new ConsoleAction
            {
                Name = "Search",
                Description = "Search files",
                Action = Search
            });

            actions.Add(new()
            {
                Name = "Exit",
                Description = "Exit files view",
                Action = () =>
                {
                    Exit();
                    return Task.CompletedTask;
                }
            });

            _selector = new(actions);
        }

        private async Task<DirectoryData?> GetDirectoryInfoAsync(string relativePath)
        {
            await _connection.Socket.SendMessageAsync(Headers.GetDirectoryInfoCommand, relativePath);

            var responseMessage = await _connection.Socket.ReceiveMessageAsync();

            if (responseMessage.Header != Headers.GetDirectoryInfoResponse)
            {
                return null;
            }

            var response = responseMessage.GetBodyAs<Response<DirectoryData>>();

            if (response?.Status != ResponseStatus.Success)
            {
                return null;
            }

            return response.GetBody()?.Match<DirectoryData?>
            (
                info => info,
                data =>
                {
                    Console.WriteLine("Couldn't receive response");
                    Console.WriteLine("Reason: " + data.Message);
                    return null;
                }
            );
        }

        private void AddPathToStack(string path)
        {
            _pathStack.Push(path.Trim().Trim("/\\".ToCharArray()));
        }

        private Task OpenPath(string path)
        {
            AddPathToStack(path);
            return UpdateDirectoryInfo();
        }

        private Task OpenPreviousPath()
        {
            if(!_pathStack.TryPop(out _) || !_pathStack.Any())
            {
                Exit();
                return Task.CompletedTask;
            }

            return UpdateDirectoryInfo();
        }

        private string BuildStackPath()
        {
            return string.Join('/', _pathStack.Reverse())
                .TrimEnd("/\\".ToCharArray()) + '/';
        }

        private string BuildFilePath(string fileName)
        {
            return BuildStackPath() + fileName;
        }

        private async Task UpdateDirectoryInfo()
        {
            do
            {
                var path = BuildStackPath();

                _currentDirectoryInfo = await GetDirectoryInfoAsync(path);

                if (_currentDirectoryInfo is not null)
                {
                    break;
                }

                Console.WriteLine("Couldn't load directory at " + path);
                Console.ReadKey();
            } while (_pathStack.Any() && _pathStack.Pop() != string.Empty);

            if (_currentDirectoryInfo is null)
            {
                Console.WriteLine("Couldn't load any directory");
                Console.WriteLine("Press any key to exit");

                Exit();
                Console.ReadKey();

                return;
            }

            UpdateSelector(_currentDirectoryInfo);
        }

        private Task ReadFile(string fileName)
        {
            var path = BuildFilePath(fileName);
            return new ConsoleFileReader(_connection).Read(path);
        }

        private async Task Search()
        {
            var pick = await SearchPicker.PickFileAsync(_connection);

            if (pick is null)
            {
                return;
            }

            var (type, path) = pick.Value;

            switch (type)
            {
                case FilePickType.Directory:
                    _pathStack.Clear();

                    AddPathToStack(string.Empty);
                    foreach (var segment in path.Split('/'))
                    {
                        AddPathToStack(segment);
                    }

                    await UpdateDirectoryInfo();
                    break;
                case FilePickType.File:
                    await ReadFile(path);
                    break;
            }
        }

        private void Exit()
        {
            _running = false;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
