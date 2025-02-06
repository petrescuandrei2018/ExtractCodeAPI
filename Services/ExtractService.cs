using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SevenZipExtractor;
using Microsoft.Extensions.Logging;

namespace ExtractCodeAPI.Services
{
    public class ExtractService
    {
        private readonly ILogger<ExtractService> _logger;

        public ExtractService(ILogger<ExtractService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// ✅ Extrage arhiva și returnează folderul destinație, excluzând fișierele `.git` și `node_modules`.
        /// </summary>
        public async Task<string> ExtractArchiveAsync(string archivePath)
        {
            string extractFolder = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(archivePath)}_{Guid.NewGuid()}");

            if (Directory.Exists(extractFolder))
                Directory.Delete(extractFolder, true);

            Directory.CreateDirectory(extractFolder);
            _logger.LogInformation($"📂 Creăm folder de extracție: {extractFolder}");

            using var archive = new ArchiveFile(archivePath);
            var files = archive.Entries.Where(e => !e.IsFolder && !e.FileName.Contains(".git") && !e.FileName.Contains("node_modules")).ToList();
            _logger.LogInformation($"📦 Arhiva conține {files.Count} fișiere utile.");

            if (files.Count == 0)
            {
                throw new Exception("❌ Arhiva nu conține fișiere valide!");
            }

            var errors = new ConcurrentBag<string>();
            var tasks = files.Select(async entry =>
            {
                try
                {
                    string safeFileName = entry.FileName.Replace("..", "_");
                    string filePath = Path.Combine(extractFolder, safeFileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                    _logger.LogInformation($"📂 Se extrage: {entry.FileName}");

                    using var memoryStream = new MemoryStream();
                    entry.Extract(memoryStream);

                    await File.WriteAllBytesAsync(filePath, memoryStream.ToArray());

                    _logger.LogInformation($"✅ Extras cu succes: {entry.FileName}");
                }
                catch (Exception ex)
                {
                    errors.Add($"⚠ Eroare la extragere: {entry.FileName} -> {ex.Message}");
                    _logger.LogError($"⚠ Eroare la extragere: {entry.FileName} -> {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);

            if (!errors.IsEmpty)
            {
                _logger.LogError("❌ Erori la extragerea unor fișiere: " + string.Join("\n", errors));
            }

            _logger.LogInformation("🎉 Extragerea finalizată cu succes!");
            return extractFolder;
        }

        /// <summary>
        /// ✅ Generează fișierul text cu codul sursă, excluzând directoarele `.git` și `node_modules`.
        /// </summary>
        public async Task<string> GenerateCodeFileAsync(string extractFolder)
        {
            string outputFile = Path.Combine(Path.GetTempPath(), "export_cod.txt");
            _logger.LogInformation($"✏ Creăm fișier: {outputFile}");

            string[] extensii = { "*.cs", "*.js", "*.html", "*.css", "*.json", "*.xml", "*.config" };
            List<string> files = extensii.SelectMany(ext => Directory.GetFiles(extractFolder, ext, SearchOption.AllDirectories)
                .Where(f => !f.Contains(".git") && !f.Contains("node_modules")))
                .ToList();

            _logger.LogInformation($"📂 Se vor scrie {files.Count} fișiere în {outputFile}");

            if (files.Count == 0)
            {
                throw new Exception("❌ Nu există fișiere de cod valide în folderul extras!");
            }

            var writeQueue = new ConcurrentQueue<string>();
            var writerTask = Task.Run(async () =>
            {
                using var stream = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                using var writer = new StreamWriter(stream, Encoding.UTF8);

                while (!writeQueue.IsEmpty)
                {
                    if (writeQueue.TryDequeue(out string line))
                    {
                        await writer.WriteLineAsync(line);
                    }
                }
            });

            await Parallel.ForEachAsync(files, async (file, token) =>
            {
                try
                {
                    string content = await File.ReadAllTextAsync(file, token);
                    writeQueue.Enqueue($"\n===== INCEPUT FISIER: {Path.GetFileName(file)} =====\n");
                    writeQueue.Enqueue(content.Trim());
                    writeQueue.Enqueue($"\n===== SFARSIT FISIER: {Path.GetFileName(file)} =====\n");
                    _logger.LogInformation($"✏ Scris: {Path.GetFileName(file)}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"⚠ Eroare la scriere: {file} -> {ex.Message}");
                }
            });

            await writerTask;

            // Verificăm dacă fișierul generat este gol.
            if (new FileInfo(outputFile).Length == 0)
            {
                throw new Exception("❌ Fișierul generat este gol.");
            }

            return outputFile;
        }
    }
}
