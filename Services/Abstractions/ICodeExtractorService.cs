using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Abstractions
{
    public interface ICodeExtractorService
    {
        Task<Dictionary<string, string>> ExtractCodeFromArchive(Stream zipStream);
    }
}
