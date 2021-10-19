using System;

namespace AOS.Common.DataSerialization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SOrderAttribute : Attribute
    {
        public int Order { get; }

        public SOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
