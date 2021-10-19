using System;
using System.IO;
using AOS.Common.Interfaces;

namespace AOS.Common.Implementations
{
    public class PathProvider : IPathProvider
    {
        public string RootPath { get; }

        public string ContentPath { get; }

        public string LogsPath { get; }

        public PathProvider()
        {
            RootPath = AppContext.BaseDirectory ?? throw new FileLoadException("Couldn't load base directory");
            ContentPath = Path.Combine(RootPath, "Content");
            LogsPath = Path.Combine(ContentPath, "Logs");

            Directory.CreateDirectory(ContentPath);
            Directory.CreateDirectory(LogsPath);
        }
    }
}
