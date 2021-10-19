using System.Threading.Tasks;
using AOS.Common.Interfaces;
using AOS.Common.Models;

namespace AOS.Common.Implementations
{
    public abstract class ProgramBase : IProgram
    {
        public ProgramContext Context { get; set; } = default!;

        public abstract Task RunAsync();
    }
}
