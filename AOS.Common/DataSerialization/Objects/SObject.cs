using System.Collections.Generic;

namespace AOS.Common.DataSerialization.Objects
{
    public class SObject
    {
        public List<object> Fields { get; init;  } = new();
    }
}
