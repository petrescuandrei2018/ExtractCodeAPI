using ExtractCodeAPI.Services.Abstractions;
using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Facade
{
    public class ExtractFacade : IExtractFacade
    {
        private readonly IFileService _fileService;
        private readonly IFileExtractionService _fileExtractionService;
        private readonly ICodeGenerationService _codeGenerationService;

        public ExtractFacade(
            IFileService fileService,
            IFileExtractionService fileExtractionService,
            ICodeGenerationService codeGenerationService)
        {
            _fileService = fileService;
            _fileExtractionService = fileExtractionService;
            _codeGenerationService = codeGenerationService;
        }

        public async Task<string> ProcessFileAsync(string archivePath)
        {
            // ✅ Extrage fișierul din arhivă
            string extractFolder = await _fileExtractionService.ExtractArchiveAsync(archivePath);

            // ✅ Generează fișierul de cod din extragere
            string outputFile = await _codeGenerationService.GenerateCodeFileAsync(extractFolder);

            return outputFile;
        }
    }
}
