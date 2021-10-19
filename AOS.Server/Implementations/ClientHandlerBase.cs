using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AOS.Common.DataSerialization;
using AOS.Common.Extensions;
using AOS.Common.Models;
using AOS.Common.Models.Data;
using AOS.Common.Models.Responses;
using AOS.Server.Attributes;
using AOS.Server.Interfaces;
using AOS.Server.Models;
using Microsoft.Extensions.Logging;

namespace AOS.Server.Implementations
{
    public abstract class ClientHandlerBase : IClientHandler, IDisposable
    {
        private readonly ILogger<ClientHandlerBase> _logger;

        protected string ClientId { get; private set; } = "???";
        protected Socket Socket { get; private set; } = default!;

        private readonly Dictionary<string, Func<Message, Task>> _handlers;

        protected ClientHandlerBase(ILogger<ClientHandlerBase> logger)
        {
            _logger = logger;
            _handlers = GetHandlers(this);
        }

        public async Task Handle(Socket socket, CancellationToken cancellationToken)
        {
            ClientId = Guid.NewGuid().ToString();
            Socket = socket;

            Log(LogLevel.Information, "Started handling client");

            int retryCount = 0;
            while (socket.Connected && !cancellationToken.IsCancellationRequested && socket.CheckConnected())
            {
                if (retryCount >= 5)
                {
                    Log(LogLevel.Warning, "Received 5 invalid messages from client in a row");
                    break;
                }

                var message = await TryReceiveMessageAsync();

                if (message == null)
                {
                    retryCount++;
                    continue;
                }

                retryCount = 0;

                Log(LogLevel.Information, $"Received message:\n\tHeader: {message.Header}\n\tBody length: {message.BodyBytes.Length.ToString()}");

                var commandHandler = _handlers.GetValueOrDefault(message.Header.ToLower());

                if (commandHandler == null)
                {
                    Log(LogLevel.Warning, $"No handler found for header: {message.Header}");
                    continue;
                }

                try
                {
                    await commandHandler.Invoke(message);
                }
                catch (Exception)
                {
                    Log(LogLevel.Warning, $"Error occurred while processing command, header: {message.Header}");
                }
            }

            StopHandling();
        }

        private void StopHandling()
        {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Disconnect(false);
            Socket.Close();
            _logger.LogInformation("Finished handling client id: {Guid}", ClientId);
        }

        protected void Log(LogLevel level, string message)
        {
            _logger.Log(level, "[{ID}] {Message}", ClientId, message);
        }

        protected async Task<Message?> TryReceiveMessageAsync()
        {
            try
            {
                return await Socket.ReceiveMessageAsync();
            }
            catch (MessageTransportException transportException)
            {
                Log(LogLevel.Warning, $"Couldn't receive message\n\tReason: {transportException.Message}");
                return null;
            }
        }

        protected async Task SendErrorResponseAsync(string header, string message)
        {
            var response = Response<object>.CreateError(new ErrorData(message));
            await Socket.SendMessageAsync(header, response);
        }

        protected async Task SendDataResponseAsync<T>(string header, T data) where T : class
        {
            var response = Response<T>.Create(data);
            await Socket.SendMessageAsync(header, response);
        }

        private static Dictionary<string, Func<Message, Task>> GetHandlers(ClientHandlerBase instance)
        {
            var handlerMethods = instance
                .GetType()
                .GetMethods();

            var handlers = new Dictionary<string, Func<Message, Task>>();

            foreach (var handlerInfo in handlerMethods)
            {
                var handlerAttribute =
                    handlerInfo.GetCustomAttribute(typeof(CommandHandlerAttribute)) as CommandHandlerAttribute;

                if (handlerAttribute is null || string.IsNullOrWhiteSpace(handlerAttribute.Header))
                {
                    continue;
                }

                if (!handlerInfo.ReturnType.IsAssignableTo(typeof(Task)))
                {
                    throw new ClientHandlerInitializationException("Handler must return Task");
                }

                var handlerParameters = handlerInfo.GetParameters();

                if (handlerParameters.Length == 0)
                {
                    handlers.Add(
                        handlerAttribute.Header.ToLower(),
                        _ => (Task)handlerInfo.Invoke(instance, Array.Empty<object?>())!
                    );
                    continue;
                }

                if (handlerParameters.Length > 1)
                {
                    throw new ClientHandlerInitializationException("Handler can only have one parameter");
                }

                handlers.Add(
                    handlerAttribute.Header.ToLower(),
                    message =>
                    {
                        var paramType = handlerInfo.GetParameters().First().ParameterType;
                        object? param = SConverter.Deserialize(paramType, message.BodyBytes);

                        if (param == null)
                        {
                            return Task.CompletedTask;
                        }

                        return (Task)handlerInfo.Invoke(instance, new[] { param } )!;
                    }
                );
            }

            return handlers;
        }

        public void Dispose()
        {
            Socket.Dispose();
        }
    }
}
