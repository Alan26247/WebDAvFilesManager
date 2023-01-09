namespace WebDavFilesRepository.Shared.Models
{
    public class FileModel
    {
        public string Url { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public long? Size { get; set; }
        public string Ext { get; set; } = string.Empty;
    }
}
