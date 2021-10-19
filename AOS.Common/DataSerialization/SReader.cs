using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AOS.Common.DataSerialization.Objects;
using OneOf;

namespace AOS.Common.DataSerialization
{
    public class SReader : IDisposable
    {
        private readonly BinaryReader _innerReader;

        public SReader(Stream stream)
        {
            _innerReader = new BinaryReader(stream);
        }

        private byte ReadByte() => _innerReader.ReadByte();

        private int ReadInt() => _innerReader.ReadInt32();

        private String ReadString() => _innerReader.ReadString();

        private SObject ReadObject()
        {
            var result = new SObject();

            var fieldsCount = _innerReader.ReadInt32();

            for (int i = 0; i < fieldsCount; ++i)
            {
                result.Fields.Add(ReadUnknown().Value);
            }

            return result;
        }

        private List<object> ReadArray()
        {
            var count = _innerReader.ReadInt32();

            return Enumerable.Range(0, count).Select(_ => ReadUnknown().Value).ToList();
        }

        public OneOf<SObject, byte, int, string, List<Object>> ReadUnknown()
        {
            var type = (SType)_innerReader.ReadByte();

            return type switch {
                SType.Object => ReadObject(),
                SType.Byte => ReadByte(),
                SType.Int => ReadInt(),
                SType.String => ReadString(),
                SType.Array => ReadArray(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), "Type: " + type)
            };
        }

        public void Dispose()
        {
            _innerReader.Dispose();
        }
    }
}
