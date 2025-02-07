using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Abstractions
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file);
    }
}
