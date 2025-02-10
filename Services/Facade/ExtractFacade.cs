using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ExtractCodeAPI.Services.Abstractions;

namespace ExtractCodeAPI.Services.Facade
{
    public class ExtractFacade : IExtractFacade
    {
        private readonly ICodeExtractorService _codeExtractorService;

        public ExtractFacade(ICodeExtractorService codeExtractorService)
        {
            _codeExtractorService = codeExtractorService;
        }

        public async Task<Dictionary<string, string>> ExtractCodeFromArchive(Stream archiveStream)
        {
            return await _codeExtractorService.ExtractCodeFromArchive(archiveStream);
        }
    }
}
