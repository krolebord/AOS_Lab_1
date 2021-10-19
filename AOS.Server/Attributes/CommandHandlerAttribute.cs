using System;

namespace AOS.Server.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandHandlerAttribute : Attribute
    {
        public string Header { get; init; }

        public CommandHandlerAttribute(string header)
        {
            Header = header;
        }
    }
}
