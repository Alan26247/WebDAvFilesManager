using System.ComponentModel.DataAnnotations;

namespace WebDavFilesRepository.Shared.Forms
{
    public class FormCreateDirectory : FormUrl
    {
        [Required]
        [MaxLength(32, ErrorMessage = "Длина имени папки должна быть не более 32 символов")]
        public string Name { get; set; }
    }
}
