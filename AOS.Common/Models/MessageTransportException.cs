using System;

namespace AOS.Common.Models
{
    public class MessageTransportException : Exception
    {
        public MessageTransportException(string message) : base(message) {}
    }
}
