using ExtractCodeAPI.Services.Abstractions;
using System;
using System.Collections.Generic;

namespace ExtractCodeAPI.Services.Implementations
{
    public class LogService : ILogService
    {
        private static readonly List<string> _logEntries = new();

        public void Log(string message)
        {
            _logEntries.Add(message);
            Console.WriteLine(message);
        }

        public List<string> GetLogs() => new List<string>(_logEntries);

        public void ClearLogs() => _logEntries.Clear();
    }
}
