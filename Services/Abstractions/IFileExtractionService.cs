using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Abstractions
{
    public interface IFileExtractionService
    {
        Task<string> ExtractArchiveAsync(string archivePath);
    }
}
