using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ExtractCodeAPI.Services
{
    public class FileService
    {
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            string tempPath = Path.GetTempPath();
            string filePath = Path.Combine(tempPath, file.FileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"✅ Fișier salvat: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"⚠ Eroare la salvarea fișierului: {ex.Message}");
                throw;
            }
        }
    }
}
