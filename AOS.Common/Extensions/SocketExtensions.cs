using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AOS.Common.DataSerialization;
using AOS.Common.Models;

namespace AOS.Common.Extensions
{
    public static class SocketExtensions
    {
        public static async Task<Message> ReceiveMessageAsync(this Socket socket)
        {
            var buffer = new byte[1024 * 4];

            bool received;
            try
            {
                received = await socket.ReceiveAsync(new Memory<byte>(buffer, 0, 1), SocketFlags.None) > 0;
            }
            catch (SocketException e)
            {
                throw new MessageTransportException(e.Message);
            }

            var expectedHeaderLength = buffer.First();

            if (!received || expectedHeaderLength == 0)
            {
                throw new MessageTransportException("Received empty message");
            }

            var headerLength =
                await socket.ReceiveAsync(new Memory<byte>(buffer, 1, expectedHeaderLength), SocketFlags.None);

            if (headerLength != expectedHeaderLength)
            {
                throw new MessageTransportException("Received invalid header");
            }

            var header = Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, 1, headerLength));

            var bodyLengthBytes =
                await socket.ReceiveAsync(new Memory<byte>(buffer, headerLength + 1, 4), SocketFlags.None);

            if (bodyLengthBytes != 4)
            {
                throw new MessageTransportException("Received invalid body length" + " " + bodyLengthBytes + " " + socket.Available);
            }

            var remainingBodyLength =
                BinaryPrimitives.ReadInt32LittleEndian(new ReadOnlySpan<byte>(buffer, headerLength + 1, 4));

            await using var bodyStream = new MemoryStream();

            while (remainingBodyLength > 0)
            {
                var bytesToRead = Math.Min(remainingBodyLength, buffer.Length);
                var bytesReceived =
                    await socket.ReceiveAsync(new Memory<byte>(buffer, 0, bytesToRead), SocketFlags.None);

                if (bytesReceived < bytesToRead)
                {
                    throw new MessageTransportException("Received invalid body");
                }

                bodyStream.Write(new Span<byte>(buffer, 0, bytesReceived));
                remainingBodyLength -= bytesReceived;
            }

            return new Message(header, bodyStream.ToArray());
        }

        public static Task SendMessageAsync(this Socket socket, string header, object? body = null)
        {
            var bodyBytes = body == null ? Array.Empty<byte>() : SConverter.Serialize(body);

            return socket.SendMessageAsync(header, bodyBytes);
        }

        public static Task SendMessageAsync(this Socket socket, string header, byte[] bodyBytes)
        {
            var message = new Message(header, bodyBytes);

            return socket.SendAsync(message.ToBytes(), SocketFlags.None);
        }

        public static bool CheckConnected(this Socket socket) =>
            !(socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0);
    }
}
