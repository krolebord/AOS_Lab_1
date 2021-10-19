using System.Collections.Generic;
using AOS.Common.DataSerialization;

namespace AOS.Common.Models.Data
{
    public class SearchData
    {
        [SOrder(1)]
        public List<string> Directories { get; set; } = new ();

        [SOrder(2)]
        public List<string> Files { get; set; } = new ();
    }
}
