using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExtractCodeAPI.Services.Abstractions;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace ExtractCodeAPI.Services.Implementations
{
    public class WebSocketHandler : IWebSocketHandler
    {
        private readonly IExtractFacade _extractFacade;
        private static readonly ConcurrentBag<string> filesToDelete = new ConcurrentBag<string>();

        public WebSocketHandler(IExtractFacade extractFacade, IHostApplicationLifetime appLifetime)
        {
            _extractFacade = extractFacade;

            appLifetime.ApplicationStopping.Register(() =>
            {
                Console.WriteLine("🗑️ Curățare fișiere încărcate...");
                foreach (var file in filesToDelete)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                            Console.WriteLine($"🗑️ Șters: {file}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Eroare la ștergerea fișierului {file}: {ex.Message}");
                    }
                }
            });
        }

        public async Task HandleWebSocket(WebSocket webSocket)
        {
            string uploadFolderPath = "C:\\Uploads";
            if (!Directory.Exists(uploadFolderPath))
            {
                Directory.CreateDirectory(uploadFolderPath);
            }

            byte[] buffer = new byte[1024 * 64];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string fileNameWithExtension = Encoding.UTF8.GetString(buffer, 0, result.Count).Trim();
            string originalExtension = Path.GetExtension(fileNameWithExtension);

            if (string.IsNullOrEmpty(originalExtension))
            {
                originalExtension = ".zip"; // Default la zip dacă nu are extensie
            }

            string filePath = Path.Combine(uploadFolderPath, $"upload_{Guid.NewGuid()}{originalExtension}");

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    await fileStream.WriteAsync(buffer, 0, result.Count);
                }
                while (!result.CloseStatus.HasValue);
            }

            Console.WriteLine($"✅ Fișier primit: {filePath}");

            // Extragem fișierele din arhivă
            string extractFolder = Path.Combine(uploadFolderPath, $"extracted_{Guid.NewGuid()}");
            Directory.CreateDirectory(extractFolder);

            var extractedCode = new Dictionary<string, string>();

            using (var archiveStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                extractedCode = await _extractFacade.ExtractCodeFromArchive(archiveStream);
            }

            if (extractedCode.Count == 0)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Nu s-au găsit fișiere de cod sursă valide!", CancellationToken.None);
                return;
            }

            string outputFile = Path.Combine(Path.GetTempPath(), "export_cod.txt");

            using (var writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                foreach (var (fileName, content) in extractedCode)
                {
                    writer.WriteLine(content);
                }
            }

            Console.WriteLine($"📄 Fișier generat: {outputFile}");

            string message = "✅ Fișierul a fost generat! Poți descărca de aici: /api/extract/download";
            byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Upload și extracție completă!", CancellationToken.None);
        }
        private bool IsArchive(string filePath)
        {
            try
            {
                using var archive = ArchiveFactory.Open(filePath);
                return archive.Entries.Any(entry => !entry.IsDirectory);
            }
            catch
            {
                return false;
            }
        }

        private void DisplayExtractionProgress(int current, int total)
        {
            int barLength = 50;
            int progress = (int)((double)current / total * barLength);
            string progressBar = new string('█', progress) + new string('-', barLength - progress);
            Console.Write($"\r🔄 [Extracting] [{progressBar}] {current}/{total} fișiere extrase");
            Console.Out.Flush();
        }

        private string DetectFileType(string filePath)
        {
            byte[] buffer = new byte[8];

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.Read(buffer, 0, buffer.Length);
            }

            string hex8 = BitConverter.ToString(buffer, 0, 8).Replace("-", " ");

            return hex8 switch
            {
                "50 4B 03 04 14 00 06 00" => "DOCX/XLSX/PPTX (ZIP-based)",
                "50 4B 03 04 14 00 00 00" => "ZIP",
                "52 61 72 21 1A 07 00 00" => "RAR (v5)",
                "37 7A BC AF 27 1C" => "7Z",
                "1F 8B 08 00 00 00 00 00" => "GZ",
                "42 5A 68 39 31 41 59 26" => "BZ2",
                "89 50 4E 47 0D 0A 1A 0A" => "PNG",
                "FF D8 FF E0 00 10 4A 46" => "JPG",
                "66 74 79 70 4D 53 4E 56" => "MP4",
                "7F 45 4C 46 02 01 01 00" => "ELF 64-bit",
                "4D 5A 90 00 03 00 00 00" => "EXE",
                _ => $"Format necunoscut (Hex: {hex8})"
            };
        }
    }
}
