using System.Collections.Generic;
using System.Text.Json;
using AOS.Common.DataSerialization;

namespace AOS.Common.Models
{
    public record SerializableObject
    {
        [SOrder(1)]
        public int Num { get; set; }

        [SOrder(2)]
        public byte SmallNum { get; set; }

        [SOrder(3)]
        public string Text { get; set; } = string.Empty;

        [SOrder(4)]
        public List<SerializableSubObject> Objects { get; set; } = new();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }

    public record SerializableSubObject
    {
        [SOrder(1)]
        public int AnotherNum { get; set; }

        [SOrder(2)]
        public string AnotherText { get; set; } = string.Empty;

        [SOrder(3)]
        public SerializableNestedObject Child { get; set; } = default!;
    }

    public record SerializableNestedObject
    {
        [SOrder(1)]
        public int NestedNum { get; set; }

        [SOrder(2)] public List<string> Strings { get; set; } = new();
    }
}
