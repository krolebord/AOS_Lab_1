using System.Threading.Tasks;
using AOS.Common.Models;

namespace AOS.Common.Interfaces
{
    public interface IProgram
    {
        public ProgramContext Context { get; set; }

        public Task RunAsync();
    }
}
