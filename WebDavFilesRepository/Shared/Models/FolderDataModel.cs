namespace WebDavFilesRepository.Shared.Models
{
    public class FolderDataModel
    {
        public string Url { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool AbilityToEditContent { get; set; }
        public FolderModel[] Folders { get; set; }
        public FileModel[] Files { get; set; }
    }
}
