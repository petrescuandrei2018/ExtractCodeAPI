using ExtractCodeAPI.DTOs;
using ExtractCodeAPI.Services.Facade;
using ExtractCodeAPI.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly IFileService _fileService;
        private readonly IExtractFacade _extractFacade;
        private readonly IFileDownloadService _fileDownloadService;

        public ExtractController(
            ILogger<ExtractController> logger,
            IFileService fileService,
            IExtractFacade extractFacade,
            IFileDownloadService fileDownloadService)
        {
            _logger = logger;
            _fileService = fileService;
            _extractFacade = extractFacade;
            _fileDownloadService = fileDownloadService;
        }

        /// <summary>
        /// ✅ Încarcă arhiva și procesează extragerea codului sursă
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
                string outputFile = await _extractFacade.ProcessFileAsync(archivePath);

                return Ok(new { message = "Fișier procesat cu succes!", downloadUrl = $"/api/extract/download" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"⚠ Eroare internă: {ex.Message}");
                return StatusCode(500, $"⚠ Eroare internă: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ Descarcă fișierul text generat
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

            byte[] fileContents = _fileDownloadService.GetFileContents(outputFile);
            return File(fileContents, "application/octet-stream", "export_cod.txt");
        }
    }
}
