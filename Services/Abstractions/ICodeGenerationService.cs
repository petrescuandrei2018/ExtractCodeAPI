using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Abstractions
{
    public interface ICodeGenerationService
    {
        Task<string> GenerateCodeFileAsync(string extractFolder);
    }
}
