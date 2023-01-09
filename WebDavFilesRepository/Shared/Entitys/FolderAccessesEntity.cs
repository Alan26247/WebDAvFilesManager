namespace WebDavFilesRepository.Shared.Entitys
{
    public class FolderAccessEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FolderUri { get; set; } = string.Empty;
        public int PermissionId { get; set; }
        public string GetParentUri()
        {
            return Additions.Uri.GetParentUri(FolderUri);
        }
    }
}
