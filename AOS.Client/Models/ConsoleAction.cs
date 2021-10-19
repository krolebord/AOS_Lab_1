using System;
using System.Threading.Tasks;

namespace AOS.Client.Models
{
    public class ConsoleAction
    {
        public string Name { get; set; } = "???";

        public string Description { get; set; } = "???";

        public Func<Task> Action { get; set; } = () => Task.CompletedTask;
    }
}
