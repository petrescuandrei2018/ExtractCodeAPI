﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtractCodeAPI.Services.Abstractions;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace ExtractCodeAPI.Services.Implementations
{
    public class CodeExtractorService : ICodeExtractorService
    {
        private readonly string[] AllowedExtensions = { ".cs", ".java", ".py", ".cpp", ".c", ".js", ".ts", ".html", ".css", ".php", ".go", ".rb" };

        public async Task<Dictionary<string, string>> ExtractCodeFromArchive(Stream archiveStream)
        {
            if (archiveStream == null || archiveStream.Length == 0)
            {
                throw new InvalidOperationException("❌ Fișierul încărcat este gol sau invalid.");
            }

            archiveStream.Position = 0; // 🔄 Resetează stream-ul înainte de citire

            string tempFolder = Path.Combine(Path.GetTempPath(), $"extracted_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempFolder);
            var extractedCode = new Dictionary<string, string>();

            try
            {
                using (var archive = OpenArchiveSafely(archiveStream))
                {
                    if (archive == null || !archive.Entries.Any())
                    {
                        throw new InvalidOperationException("❌ Arhiva nu conține fișiere valide sau este coruptă.");
                    }

                    int totalFiles = archive.Entries.Count(entry => !entry.IsDirectory && AllowedExtensions.Contains(Path.GetExtension(entry.Key)));
                    int processedFiles = 0;

                    Console.WriteLine($"📂 Se extrag {totalFiles} fișiere de cod...");

                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory && AllowedExtensions.Contains(Path.GetExtension(entry.Key)))
                        {
                            string filePath = Path.Combine(tempFolder, entry.Key);
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                            using (var entryStream = entry.OpenEntryStream())
                            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                            {
                                await entryStream.CopyToAsync(fileStream);
                            }

                            string fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);

                            // ✅ Eliminăm fișierele goale
                            if (string.IsNullOrWhiteSpace(fileContent))
                            {
                                Console.WriteLine($"⚠ Fișierul {entry.Key} este gol și a fost ignorat.");
                                continue;
                            }

                            string markedContent = $"===== Start: {entry.Key} =====\n{fileContent}\n===== End: {entry.Key} =====";
                            extractedCode[entry.Key] = markedContent;

                            processedFiles++;
                            DisplayProgressBar(processedFiles, totalFiles);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Eroare la procesarea arhivei: {ex.Message}");
                throw;
            }
            finally
            {
                Directory.Delete(tempFolder, true);
            }

            return extractedCode;
        }

        // ✅ Funcție sigură pentru deschiderea arhivelor
        private IArchive OpenArchiveSafely(Stream archiveStream)
        {
            try
            {
                return ArchiveFactory.Open(archiveStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Eroare la deschiderea arhivei: {ex.Message}");
                return null;
            }
        }

        // 📊 Funcție pentru progress bar
        private void DisplayProgressBar(int current, int total)
        {
            int barLength = 50;
            int progress = (int)((double)current / total * barLength);
            string progressBar = new string('█', progress) + new string('-', barLength - progress);
            Console.Write($"\r[{progressBar}] {current}/{total} fișiere procesate");
            Console.Out.Flush();
        }
    }
}
