using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Abstractions
{
    public interface IExtractFacade
    {
        Task<string> ProcessFileAsync(string archivePath);
    }
}
