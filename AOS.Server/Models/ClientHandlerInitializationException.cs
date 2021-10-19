using System;

namespace AOS.Server.Models
{
    public class ClientHandlerInitializationException : Exception
    {
        public ClientHandlerInitializationException(string message) : base(message) { }
    }
}
