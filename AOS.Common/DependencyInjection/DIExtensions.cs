using System;
using Microsoft.Extensions.Configuration;

namespace AOS.Common.DependencyInjection
{
    public static class DIExtensions
    {
        public static IConfigurationBuilder AddDefaultConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("properties.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}
