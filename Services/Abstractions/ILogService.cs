using System.Collections.Generic;

namespace ExtractCodeAPI.Services.Abstractions
{
    public interface ILogService
    {
        void Log(string message);
        List<string> GetLogs();
        void ClearLogs();
    }
}
