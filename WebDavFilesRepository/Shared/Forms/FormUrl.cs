using System.ComponentModel.DataAnnotations;

namespace WebDavFilesRepository.Shared.Forms
{
    public class FormUrl
    {
        [Required]
        public string Uri { get; set; }
    }
}
