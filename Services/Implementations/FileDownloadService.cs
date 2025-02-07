using ExtractCodeAPI.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.IO;

namespace ExtractCodeAPI.Services.Implementations
{
    public class FileDownloadService : IFileDownloadService
    {
        private readonly ILogger<FileDownloadService> _logger;

        public FileDownloadService(ILogger<FileDownloadService> logger)
        {
            _logger = logger;
        }

        public byte[] GetFileContents(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"⚠ Fișierul nu există: {filePath}");
                return null;
            }

            return File.ReadAllBytes(filePath);
        }
    }
}
