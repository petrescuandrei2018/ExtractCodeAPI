using Microsoft.AspNetCore.Http;
using System.IO;

namespace ExtractCodeAPI.Services.Implementations
{
    public class FileValidatorService
    {
        public bool IsValidFile(IFormFile file, out string errorMessage)
        {
            if (file == null || file.Length == 0)
            {
                errorMessage = "⚠ Trebuie să încarci un fișier arhivat.";
                return false;
            }

            string extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".zip" && extension != ".rar")
            {
                errorMessage = "⚠ Format invalid. Acceptat: .zip, .rar";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
