using ExtractCodeAPI.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Implementations
{
    public class FileService : IFileService
    {
        public async Task<string> SaveFileAsync(IFormFile file)
        {
            string filePath = Path.Combine(Path.GetTempPath(), file.FileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return filePath;
        }
    }
}
