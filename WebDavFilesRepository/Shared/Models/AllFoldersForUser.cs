namespace WebDavFilesRepository.Shared.Models
{
    public class AllFoldersForUser
    {
        public int UserId { get; set; }

        public FolderWithPermissionsModel RootFolder { get; set; }
    }
}
