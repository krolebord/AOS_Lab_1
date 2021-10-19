using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AOS.Common.Interfaces
{
    public interface IProgramBuilder<TProgram> where TProgram : class, IProgram
    {
        IProgramBuilder<TProgram> SetConfiguration(Action<IConfigurationBuilder> buildConfiguration);

        IProgramBuilder<TProgram> ConfigureServices(Action<IServiceCollection> buildServices);

        IProgram Build();
    }
}
