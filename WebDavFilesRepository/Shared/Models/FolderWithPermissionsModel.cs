namespace WebDavFilesRepository.Shared.Models
{
    public class FolderWithPermissionsModel : FolderModel
    {
        public bool AllowedToView { get; set; }
        public bool AllowedToEdit { get; set; }
        public FolderWithPermissionsModel[] Folders { get; set; }
    }
}
