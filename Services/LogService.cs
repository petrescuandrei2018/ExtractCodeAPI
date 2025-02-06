using System.Collections.Generic;

namespace ExtractCodeAPI.Services
{
    public class LogService
    {
        private static readonly List<string> _logEntries = new List<string>();

        public void Log(string message)
        {
            _logEntries.Add(message);
            Console.WriteLine(message);
        }

        public List<string> GetLogs() => _logEntries;
        public void ClearLogs() => _logEntries.Clear();
    }
}
