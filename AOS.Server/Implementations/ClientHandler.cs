using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AOS.Common.Constants;
using AOS.Common.Extensions;
using AOS.Common.Interfaces;
using AOS.Common.Models;
using AOS.Common.Models.Data;
using AOS.Server.Attributes;
using Microsoft.Extensions.Logging;

namespace AOS.Server.Implementations
{
    public class ClientHandler : ClientHandlerBase
    {
        private readonly IPathProvider _pathProvider;

        public ClientHandler(ILogger<ClientHandler> logger, IPathProvider pathProvider) : base(logger)
        {
            _pathProvider = pathProvider;
        }

        [CommandHandler(Headers.WhoCommand)]
        public async Task Who()
        {
            var result = "Kiril Shevchuk K-24 Lab 1 Var 6 List of files";
            await Socket.SendMessageAsync(Headers.WhoResponse, result);
        }

        [CommandHandler(Headers.SerializationExample)]
        public async Task SerializationExample(SerializableObject obj)
        {
            await Socket.SendMessageAsync(Headers.SerializationExample, obj);
        }

        [CommandHandler(Headers.GetDirectoryInfoCommand)]
        public async Task GetDirectoryInfo(string relativePath)
        {
            var path = GetValidUriOrDefault(relativePath);

            if (path is null || !Directory.Exists(path.AbsolutePath))
            {
                await SendErrorResponseAsync(Headers.GetDirectoryInfoResponse, "Invalid relative path");
                return;
            }

            Log(LogLevel.Information, "Accessed path: " + path.AbsolutePath);


            var directoryInfo = new DirectoryData
            {
                RootPath = _pathProvider.ContentPath,
                RelativePath = relativePath,
                Directories = Directory.EnumerateDirectories(path.AbsolutePath)
                    .Select(Path.GetFileName)
                    .Where(x => x is not null)
                    .ToList()!,
                Files = Directory.EnumerateFiles(path.AbsolutePath)
                    .Select(Path.GetFileName)
                    .Where(x => x is not null)
                    .ToList()!
            };

            await SendDataResponseAsync(Headers.GetDirectoryInfoResponse, directoryInfo);
        }

        [CommandHandler(Headers.ReadFileCommand)]
        public async Task ReadFile(string relativePath)
        {
            var path = GetValidUriOrDefault(relativePath);

            Log(LogLevel.Information, "Accessed path: " + path?.AbsolutePath);

            if (path is null || !File.Exists(path.AbsolutePath))
            {
                await SendErrorResponseAsync(Headers.ReadFileResponse, "Invalid relative path");
                return;
            }

            var fileData = new FileData
            {
                FileName = Path.GetFileNameWithoutExtension(path.AbsolutePath),
                Extension = Path.GetExtension(path.AbsolutePath),
                EncodedContent = Convert.ToBase64String(await File.ReadAllBytesAsync(path.AbsolutePath))
            };

            await SendDataResponseAsync(Headers.ReadFileResponse, fileData);
        }

        [CommandHandler(Headers.FilterFilesCommand)]
        public async Task SearchFiles(string filter)
        {
            var path = _pathProvider.ContentPath;

            var baseUri = new Uri(_pathProvider.ContentPath + '/');

            var directories = Directory
                .EnumerateDirectories(path, filter, SearchOption.AllDirectories)
                .Select(x => baseUri.MakeRelativeUri(new Uri(x)).ToString());

            var files = Directory
                .EnumerateFiles(path, filter, SearchOption.AllDirectories)
                .Select(x => baseUri.MakeRelativeUri(new Uri(x)).ToString());


            var searchData = new SearchData
            {
                Directories = directories.ToList(),
                Files = files.ToList()
            };

            await SendDataResponseAsync(Headers.FilterFilesResponse, searchData);
        }

        public Uri? GetValidUriOrDefault(string relativePath)
        {
            if (!relativePath.StartsWith('/') && !relativePath.StartsWith('\\'))
            {
                relativePath = Path.DirectorySeparatorChar + relativePath;
            }

            var path = _pathProvider.ContentPath + relativePath;

            var rootUri = new Uri(_pathProvider.ContentPath);
            var pathUri = new Uri(path);

            if (rootUri == pathUri || !rootUri.IsBaseOf(pathUri))
            {
                return null;
            }

            return pathUri;
        }
    }
}
