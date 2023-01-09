using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebDavFilesRepository.Shared.Forms
{
    public class FormUploadFiles
    {
        [Required]
        public string Uri { get; set; }
        [Required]
        public IFormFile File { get; set; }
    }
}
