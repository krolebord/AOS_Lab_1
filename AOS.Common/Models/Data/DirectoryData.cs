using System.Collections.Generic;
using AOS.Common.DataSerialization;

namespace AOS.Common.Models.Data
{
    public class DirectoryData
    {
        [SOrder(1)]
        public string RootPath { get; set; } = string.Empty;

        [SOrder(2)]
        public string RelativePath { get; set; } = string.Empty;

        [SOrder(3)]
        public List<string> Directories { get; set; } = new();

        [SOrder(4)]
        public List<string> Files { get; set; } = new();
    }
}
