namespace AOS.Common.Interfaces
{
    public interface IPathProvider
    {
        public string RootPath { get; }

        public string ContentPath { get; }

        public string LogsPath { get; }
    }
}
