using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AOS.Server.Interfaces
{
    public interface IClientHandler
    {
        public Task Handle(Socket socket, CancellationToken cancellationToken);
    }
}
