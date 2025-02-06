using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ExtractCodeAPI.DTOs
{
    public class FileUploadDto
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
