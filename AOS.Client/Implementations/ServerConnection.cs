using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace AOS.Client.Implementations
{
    public class ServerConnection : IDisposable
    {
        private readonly ILogger<ServerConnection> _logger;

        public Socket Socket { get; }

        public ServerConnection(ILogger<ServerConnection> logger, Socket socket)
        {
            _logger = logger;
            Socket = socket;

            _logger.LogInformation("Started server connection");
        }

        public void Dispose()
        {
            _logger.LogInformation("Closing server connection");
            Socket.Dispose();
        }
    }
}
