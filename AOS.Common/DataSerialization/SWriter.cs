using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AOS.Common.DataSerialization.Objects;

namespace AOS.Common.DataSerialization
{
    public class SWriter : IDisposable
    {
        private readonly BinaryWriter _innerWriter;

        public SWriter(Stream stream)
        {
            _innerWriter = new BinaryWriter(stream);
        }

        private void WriteObject(SObject value)
        {
            WriteType(SType.Object);

            _innerWriter.Write(value.Fields.Count);

            foreach (object field in value.Fields)
            {
                Write(field);
            }
        }

        private void WriteByte(byte value)
        {
            WriteType(SType.Byte);
            _innerWriter.Write(value);
        }

        private void WriteInt(int value)
        {
            WriteType(SType.Int);
            _innerWriter.Write(value);
        }

        private void WriteString(string value)
        {
            WriteType(SType.String);
            _innerWriter.Write(value);
        }

        public void Write(object value)
        {
            switch (value)
            {
                case byte x:
                    WriteByte(x);
                    break;
                case int x:
                    WriteInt(x);
                    break;
                case string x:
                    WriteString(x);
                    break;
                case SObject x:
                    WriteObject(x);
                    break;
                case IEnumerable<object> x:
                    WriteArray(x);
                    break;
                default: throw new ArgumentException($"Unsupported type: {value.GetType()}", nameof(value));
            }
        }

        private void WriteArray(IEnumerable<object> value)
        {
            WriteType(SType.Array);

            var objects = value.ToList();

            _innerWriter.Write(objects.Count);

            foreach (object obj in objects)
            {
                Write(obj);
            }
        }

        private void WriteType(SType type)
        {
            _innerWriter.Write((byte)type);
        }

        public void Dispose()
        {
            _innerWriter.Dispose();
        }
    }
}
