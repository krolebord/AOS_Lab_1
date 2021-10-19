using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AOS.Client.Utils;
using AOS.Common.Constants;
using AOS.Common.Extensions;
using AOS.Common.Models.Data;
using AOS.Common.Models.Responses;

namespace AOS.Client.Implementations
{
    public enum FilePickType
    {
        File,
        Directory
    }

    public static class SearchPicker
    {
        public static async Task<(FilePickType, string)?> PickFileAsync(ServerConnection connection)
        {
            Console.Clear();

            Console.WriteLine("Enter search filter:");

            var filter = Console.ReadLine();

            var searchData = await GetSearchData(connection, filter);

            if (searchData is null || !searchData.Directories.Any() && !searchData.Files.Any())
            {
                Console.WriteLine("No search results");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                return null;
            }

            Console.WriteLine();

            ConsoleUtils.WriteTopEdge();
            ConsoleUtils.WriteBottomEdge();

            return Pick(searchData);
        }

        private static (FilePickType, string)? Pick(SearchData searchData)
        {
            var items = new List<(FilePickType, string)>();

            items.AddRange(searchData.Directories.Select(x => (FilePickType.Directory, x)));
            items.AddRange(searchData.Files.Select(x => (FilePickType.File, x)));

            var selector = new ItemSelector<(FilePickType, string)>(items);

            while (true)
            {
                DrawPicker(selector);

                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        selector.SelectPrevious();
                        break;
                    case ConsoleKey.DownArrow:
                        selector.SelectNext();
                        break;
                    case ConsoleKey.Escape:
                        return null;
                    case ConsoleKey.Spacebar:
                        return selector.GetSelected();
                }
            }
        }

        private static void DrawPicker(ItemSelector<(FilePickType, string)> selector)
        {
            Console.Clear();

            ConsoleUtils.WriteTopEdge();

            ConsoleUtils.WriteRow("Search result".PadBoth(Console.WindowWidth - 2));

            ConsoleUtils.WriteMiddleEdge();

            foreach (var item in selector.Items)
            {
                var (type, path) = item;

                if (selector.IsSelected((type, path)))
                {
                    ConsoleUtils.WriteColoredRow(path, ConsoleColor.Black, ConsoleColor.White);
                }
                else
                {
                    ConsoleUtils.WriteRow(path);
                }
            }

            ConsoleUtils.WriteBottomEdge();
        }

        private static async Task<SearchData?> GetSearchData(ServerConnection connection, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return null;
            }

            await connection.Socket.SendMessageAsync(Headers.FilterFilesCommand, filter);

            var responseMessage = await connection.Socket.ReceiveMessageAsync();

            if (responseMessage.Header != Headers.FilterFilesResponse)
            {
                return null;
            }

            var response = responseMessage.GetBodyAs<Response<SearchData>>();

            if (response?.Status != ResponseStatus.Success)
            {
                return null;
            }

            return response.GetBody()?.Match<SearchData?>
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
    }
}
