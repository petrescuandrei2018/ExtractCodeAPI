using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Abstractions
{
    public interface IArchiveExtractor
    {
        Task ExtractAsync(string archivePath, string destinationFolder);
    }
}
