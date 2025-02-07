using ExtractCodeAPI.Services.Abstractions;
using SevenZipExtractor;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Implementations
{
    public class ZipExtractor : IArchiveExtractor
    {
        public async Task ExtractAsync(string archivePath, string destinationFolder)
        {
            using var archive = new ArchiveFile(archivePath);
            foreach (var entry in archive.Entries.Where(e => !e.IsFolder))
            {
                string filePath = Path.Combine(destinationFolder, entry.FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using var memoryStream = new MemoryStream();
                entry.Extract(memoryStream);
                await File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
            }
        }
    }
}
