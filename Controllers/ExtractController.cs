using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExtractCodeAPI.Services;
using ExtractCodeAPI.DTOs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ExtractCodeAPI.Controllers
{
    [Route("api/extract")]
    [ApiController]
    public class ExtractController : ControllerBase
    {
        private readonly ILogger<ExtractController> _logger;
        private readonly FileService _fileService;
        private readonly ExtractService _extractService;

        public ExtractController(ILogger<ExtractController> logger, FileService fileService, ExtractService extractService)
        {
            _logger = logger;
            _fileService = fileService;
            _extractService = extractService;
        }

        /// <summary>
        /// ✅ Încarcă arhiva și procesează extragerea codului sursă.
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadArchive([FromForm] FileUploadDto uploadDto)
        {
            if (uploadDto.File == null || uploadDto.File.Length == 0)
            {
                _logger.LogWarning("⚠ Fișier invalid");
                return BadRequest("Trebuie să încarci un fișier arhivat.");
            }

            try
            {
                string archivePath = await _fileService.SaveFileAsync(uploadDto.File);
                string extractFolder = await _extractService.ExtractArchiveAsync(archivePath);
                string outputFile = await _extractService.GenerateCodeFileAsync(extractFolder);

                // Returnăm link-ul de descărcare doar dacă fișierul nu este gol.
                if (new FileInfo(outputFile).Length == 0)
                {
                    _logger.LogError("⚠ Fișierul generat este gol.");
                    return BadRequest("Fișierul generat este gol.");
                }

                return Ok(new { message = "Fișier procesat cu succes!", downloadUrl = "/api/extract/download" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"⚠ Eroare internă: {ex.Message}");
                return StatusCode(500, $"⚠ Eroare internă: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ Descarcă fișierul text generat.
        /// </summary>
        [HttpGet("download")]
        public IActionResult DownloadExtractedCode()
        {
            string outputFile = Path.Combine(Path.GetTempPath(), "export_cod.txt");

            if (!System.IO.File.Exists(outputFile))
            {
                _logger.LogWarning("⚠ Fișierul nu există. Încarcă mai întâi un proiect!");
                return NotFound("Fișierul nu există. Încarcă mai întâi un proiect!");
            }

            var stream = new FileStream(outputFile, FileMode.Open, FileAccess.Read);
            return File(stream, "text/plain", "export_cod.txt");
        }
    }
}
