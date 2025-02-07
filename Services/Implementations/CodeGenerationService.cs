using ExtractCodeAPI.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Implementations
{
    public class CodeGenerationService : ICodeGenerationService
    {
        private readonly ILogger<CodeGenerationService> _logger;

        public CodeGenerationService(ILogger<CodeGenerationService> logger)
        {
            _logger = logger;
        }

        public async Task<string> GenerateCodeFileAsync(string extractFolder)
        {
            string outputFile = Path.Combine(Path.GetTempPath(), "export_cod.txt");
            _logger.LogInformation($"✏ Creăm fișier: {outputFile}");

            string[] extensii = { "*.cs", "*.js", "*.html", "*.css", "*.json", "*.xml", "*.config" };
            List<string> files = extensii
                .SelectMany(ext => Directory.GetFiles(extractFolder, ext, SearchOption.AllDirectories))
                .ToList();

            if (files.Count == 0)
            {
                throw new System.Exception("❌ Nu există fișiere de cod valide în folderul extras!");
            }

            var writeQueue = new ConcurrentQueue<string>();

            using var writer = new StreamWriter(outputFile, false, Encoding.UTF8);

            foreach (var file in files)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(file);
                    writeQueue.Enqueue($"\n===== INCEPUT FISIER: {Path.GetFileName(file)} =====\n");
                    writeQueue.Enqueue(content.Trim());
                    writeQueue.Enqueue($"\n===== SFARSIT FISIER: {Path.GetFileName(file)} =====\n");
                    _logger.LogInformation($"✏ Scris: {Path.GetFileName(file)}");
                }
                catch (System.Exception ex)
                {
                    _logger.LogError($"⚠ Eroare la scriere: {file} -> {ex.Message}");
                }
            }

            while (writeQueue.TryDequeue(out string line))
            {
                await writer.WriteLineAsync(line);
            }

            return outputFile;
        }
    }
}
