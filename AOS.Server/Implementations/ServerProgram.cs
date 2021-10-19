using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AOS.Common.Implementations;
using AOS.Common.Interfaces;
using AOS.Server.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AOS.Server.Implementations
{
    public class ServerProgram : ProgramBase, IDisposable
    {
        private readonly ILogger<ServerProgram> _logger;
        private readonly IPathProvider _pathProvider;

        private Socket? _listener;

        public ServerProgram(
            ILogger<ServerProgram> logger,
            IPathProvider pathProvider)
        {
            _logger = logger;
            _pathProvider = pathProvider;
        }

        public override async Task RunAsync()
        {
            try
            {
                StartServer();

                var cancellationTokenSource = new CancellationTokenSource();

                Console.CancelKeyPress += (_, _) => cancellationTokenSource.Cancel();

                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var client = await _listener!.AcceptAsync();

                    var handler = Context.ServiceProvider.GetRequiredService<IClientHandler>();

                    await handler.Handle(client, cancellationTokenSource.Token);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("{Error}", e);
            }
            finally
            {
                StopServer();
            }
        }

        private void StartServer()
        {
            _logger.LogInformation("Starting server...");

            var host = Dns.GetHostEntry("localhost");
            var ipAddress = host.AddressList.First();
            var localEndPoint = new IPEndPoint(ipAddress, 1031);

            _listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(localEndPoint);
            _listener.Listen();

            _logger.LogInformation("Started server at: {Endpoint}", localEndPoint);

            _logger.LogInformation(
                "Directories:\n\tRoot: {Root}\n\tContent: {Content}\n\tLogs: {Logs}",
                _pathProvider.RootPath,
                _pathProvider.ContentPath,
                _pathProvider.LogsPath
            );
        }

        private void StopServer()
        {
            _logger.LogInformation("Stopping server...");
            _listener?.Close();
        }

        public void Dispose()
        {
            _listener?.Dispose();
        }
    }
}
