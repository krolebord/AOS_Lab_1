using AOS.Common.DataSerialization;

namespace AOS.Common.Models.Data
{
    public class FileData
    {
        [SOrder(1)]
        public string FileName { get; set; } = string.Empty;

        [SOrder(2)]
        public string Extension { get; set; } = string.Empty;

        [SOrder(3)]
        public string EncodedContent { get; set; } = string.Empty;
    }
}
