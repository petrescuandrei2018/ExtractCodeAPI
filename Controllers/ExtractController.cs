using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ExtractCodeAPI.Services.Abstractions;
using ExtractCodeAPI.Services.Facade;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations; 


namespace ExtractCodeAPI.Controllers
{
    [Route("api/extract")]
    [ApiController]
    public class ExtractController : ControllerBase
    {
        private readonly ILogger<ExtractController> _logger;
        private readonly IExtractFacade _extractFacade;

        public ExtractController(
            ILogger<ExtractController> logger,
            IExtractFacade extractFacade)
        {
            _logger = logger;
            _extractFacade = extractFacade;
        }



        [HttpPost("upload")]
        [RequestSizeLimit(5L * 1024 * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadArchive([Required]IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("❌ Niciun fișier selectat!");
                Console.Out.Flush();  // ✅ Forțează afișarea în consolă
                return BadRequest(new
                {
                    message = "Trebuie să selectezi un fișier arhivat pentru a continua."
                });
            }

            try
            {
                Console.WriteLine($"📂 Fișier primit: {file.FileName}, mărime: {file.Length / (1024 * 1024)} MB");
                Console.Out.Flush();  // ✅ Forțează afișarea în consolă

                string tempFilePath = Path.Combine(Path.GetTempPath(), file.FileName);

                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                using (var inputStream = file.OpenReadStream())
                {
                    Console.WriteLine("⏳ Salvăm fișierul temporar...");
                    Console.Out.Flush();  // ✅ Forțează afișarea în consolă
                    await CopyStreamWithProgress(inputStream, fileStream, file.Length);
                }

                Console.WriteLine($"\n✅ Fișier salvat la: {tempFilePath}");
                Console.Out.Flush();  // ✅ Forțează afișarea în consolă

                using (var archiveStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true))
                {
                    Console.WriteLine("🚀 Începem extragerea fișierelor...");
                    Console.Out.Flush();  // ✅ Forțează afișarea în consolă

                    var extractedCode = await _extractFacade.ExtractCodeFromArchive(archiveStream);
                    if (extractedCode.Count == 0)
                    {
                        Console.WriteLine("⚠ Nu s-au găsit fișiere de cod sursă valide!");
                        Console.Out.Flush();
                        return BadRequest("Nu s-au găsit fișiere de cod sursă valide.");
                    }

                    string outputFile = Path.Combine(Path.GetTempPath(), "export_cod.txt");
                    using (var writer = new StreamWriter(outputFile, false, Encoding.UTF8))
                    {
                        foreach (var (fileName, content) in extractedCode)
                        {
                            writer.WriteLine(content);
                            Console.WriteLine($"📝 Fișier scris: {fileName}");
                            Console.Out.Flush();  // ✅ Forțează afișarea în consolă
                        }
                    }

                    Console.WriteLine($"✅ Fișierele de cod salvate în: {outputFile}");
                    Console.Out.Flush();  // ✅ Forțează afișarea în consolă
                    return Ok(new { message = "Fișierele de cod sursă au fost extrase!", downloadUrl = "/api/extract/download" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Eroare: {ex.Message}");
                Console.Out.Flush();  // ✅ Forțează afișarea în consolă
                return StatusCode(500, $"Eroare internă: {ex.Message}");
            }
        }





        [HttpGet("download")]
        public IActionResult DownloadExtractedCode()
        {
            string outputFile = Path.Combine(Path.GetTempPath(), "export_cod.txt");

            if (!System.IO.File.Exists(outputFile))
            {
                return BadRequest(new
                {
                    message = "Nu ai încărcat niciun fișier. Încarcă o arhivă și apoi încearcă din nou descărcarea."
                });
            }

            byte[] fileContents = System.IO.File.ReadAllBytes(outputFile);
            return File(fileContents, "text/plain", "export_cod.txt");
        }


        private async Task CopyStreamWithProgress(Stream input, Stream output, long totalBytes)
        {
            byte[] buffer = new byte[8192];  // Buffer de 8KB
            long totalRead = 0;
            int bytesRead;
            int barLength = 50; // Lungimea progress bar-ului

            while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await output.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;

                int progress = (int)((double)totalRead / totalBytes * barLength);
                string progressBar = new string('█', progress) + new string('-', barLength - progress);
                Console.Write($"\r[{progressBar}] {totalRead / (1024 * 1024)}MB / {totalBytes / (1024 * 1024)}MB");
                Console.Out.Flush();  // ✅ Forțează afișarea imediată în consolă
            }
        }

    }
}
