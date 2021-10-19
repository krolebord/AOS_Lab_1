using System;
using System.IO;
using System.Text;
using AOS.Common.DataSerialization;

namespace AOS.Common.Models
{
    public class Message
    {
        public string Header { get; }
        public byte[] BodyBytes { get; }

        public Message(string header, byte[] bytes)
        {
            Header = header;
            BodyBytes = bytes;
        }

        public Message(string header, object body)
        {
            Header = header;
            BodyBytes = SConverter.Serialize(body);
        }

        public byte[] ToBytes()
        {
            using var stream = new MemoryStream();

            var headerBytes = Encoding.UTF8.GetBytes(Header);

            if (headerBytes.Length > byte.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(Header));
            }

            stream.WriteByte((byte)headerBytes.Length);
            stream.Write(headerBytes);

            var encodedBodyLength = BitConverter.GetBytes(BodyBytes.Length);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(encodedBodyLength);
            }

            stream.Write(encodedBodyLength);
            stream.Write(BodyBytes);

            return stream.ToArray();
        }

        public T? GetBodyAs<T>()
        {
            return SConverter.Deserialize<T>(BodyBytes);
        }
    }
}
