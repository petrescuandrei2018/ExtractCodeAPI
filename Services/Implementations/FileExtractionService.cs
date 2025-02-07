using ExtractCodeAPI.Services.Abstractions;
using Microsoft.Extensions.Logging;
using SevenZipExtractor;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Implementations
{
    public class FileExtractionService : IFileExtractionService
    {
        private readonly ILogger<FileExtractionService> _logger;

        public FileExtractionService(ILogger<FileExtractionService> logger)
        {
            _logger = logger;
        }

        public async Task<string> ExtractArchiveAsync(string archivePath)
        {
            string extractFolder = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(archivePath)}_{Guid.NewGuid()}");

            Directory.CreateDirectory(extractFolder);
            _logger.LogInformation($"📂 Creăm folder de extracție: {extractFolder}");

            using var archive = new ArchiveFile(archivePath);
            var files = archive.Entries.Where(e => !e.IsFolder).ToList();

            if (files.Count == 0)
            {
                throw new Exception("❌ Arhiva nu conține fișiere valide!");
            }

            var errors = new ConcurrentBag<string>();

            foreach (var entry in files)
            {
                try
                {
                    string filePath = Path.Combine(extractFolder, entry.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    using var memoryStream = new MemoryStream();
                    entry.Extract(memoryStream);
                    await File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
                }
                catch (Exception ex)
                {
                    errors.Add($"⚠ Eroare la extragere: {entry.FileName} -> {ex.Message}");
                }
            }

            return extractFolder;
        }
    }
}
