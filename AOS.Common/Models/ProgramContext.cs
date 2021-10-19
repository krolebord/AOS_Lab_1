using System;
using Microsoft.Extensions.Configuration;

namespace AOS.Common.Models
{
    public class ProgramContext
    {
        public IConfiguration Configuration { get; set; } = default!;

        public IServiceProvider ServiceProvider { get; init; } = default!;
    }
}
