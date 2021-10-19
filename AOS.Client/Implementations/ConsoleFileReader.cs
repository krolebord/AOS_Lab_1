using System;
using System.Text;
using System.Threading.Tasks;
using AOS.Client.Utils;
using AOS.Common.Constants;
using AOS.Common.Extensions;
using AOS.Common.Models.Data;
using AOS.Common.Models.Responses;

namespace AOS.Client.Implementations
{
    public class ConsoleFileReader
    {
        private readonly ServerConnection _connection;

        public ConsoleFileReader(ServerConnection connection)
        {
            _connection = connection;
        }

        public async Task Read(string path)
        {
            Console.Clear();

            Console.WriteLine("Accessing file at "+ path);

            var fileData = await GetFileDataAsync(path);

            if (fileData is null)
            {
                Console.WriteLine("Couldn't access file");
                Exit();
                return;
            }

            Console.WriteLine();
            ConsoleUtils.WriteTopEdge();
            ConsoleUtils.WriteRow($"{fileData.FileName} UTF8 Encoded".PadBoth(Console.WindowWidth - 2));
            ConsoleUtils.WriteBottomEdge();
            Console.WriteLine();

            var contentBytes = Convert.FromBase64String(fileData.EncodedContent);
            Console.WriteLine(Encoding.UTF8.GetString(contentBytes));

            Exit();
        }

        private void Exit()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private async Task<FileData?> GetFileDataAsync(string relativePath)
        {
            await _connection.Socket.SendMessageAsync(Headers.ReadFileCommand, relativePath);

            var responseMessage = await _connection.Socket.ReceiveMessageAsync();

            if (responseMessage.Header != Headers.ReadFileResponse)
            {
                return null;
            }

            var response = responseMessage.GetBodyAs<Response<FileData>>();

            if (response?.Status != ResponseStatus.Success)
            {
                return null;
            }

            return response.GetBody()?.Match<FileData?>
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
