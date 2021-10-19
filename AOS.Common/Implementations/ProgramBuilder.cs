using System;
using System.IO;
using AOS.Common.Interfaces;
using AOS.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AOS.Common.Implementations
{
    public class ProgramBuilder<TProgram> : IProgramBuilder<TProgram>
        where TProgram : class, IProgram
    {
        private readonly IConfigurationBuilder _configurationBuilder = new ConfigurationBuilder();
        private readonly IServiceCollection _services = new ServiceCollection();

        public IProgramBuilder<TProgram> SetConfiguration(Action<IConfigurationBuilder> buildConfiguration)
        {
            buildConfiguration(_configurationBuilder);

            return this;
        }

        public IProgramBuilder<TProgram> ConfigureServices(Action<IServiceCollection> buildServices)
        {
            buildServices(_services);

            return this;
        }

        public IProgram Build()
        {
            var configuration = _configurationBuilder.Build();

            _services.AddSingleton<IConfiguration>(configuration);
            _services.AddSingleton<TProgram>();
            _services.AddSingleton<IPathProvider, PathProvider>();

            var serviceProvider = _services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            if (loggerFactory == null)
            {
                Console.WriteLine("No logging configured");
            }
            else
            {
                var pathProvider = serviceProvider.GetRequiredService<IPathProvider>();
                var logFilePath = Path.Combine(pathProvider.LogsPath, "Log.txt");
                loggerFactory.AddFile(logFilePath);
            }
            
            var program = serviceProvider.GetRequiredService<TProgram>();
            program.Context = new ProgramContext
            {
                Configuration = configuration,
                ServiceProvider = serviceProvider
            };

            return program;
        }
    }
}
