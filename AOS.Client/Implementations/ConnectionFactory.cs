using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using AOS.Common.Constants;
using Microsoft.Extensions.Logging;

namespace AOS.Client.Implementations
{
    public class ConnectionFactory
    {
        private readonly ILogger<ConnectionFactory> _logger;
        private readonly IPEndPoint _remoteEndPoint;
        private readonly IPAddress _ipAddress;

        public ConnectionFactory(ILogger<ConnectionFactory> logger)
        {
            _logger = logger;
            var host = Dns.GetHostEntry("localhost");
            _ipAddress = host.AddressList.First();
            _remoteEndPoint = new IPEndPoint(_ipAddress, TransportConstants.Port);
        }

        public async Task<ServerConnection> CreateConnectionAsync(ILogger<ServerConnection> connectionLogger)
        {
            _logger.LogInformation("Connecting to [{Address}]", _remoteEndPoint);

            var socket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(_remoteEndPoint);

            return new ServerConnection(connectionLogger, socket);
        }
    }
}
