namespace WebDavFilesRepository.Shared.Entitys
{
    public class LogEntity
    {
        public int Id { get; set; }
        public string LogEvent { get; set; } = string.Empty;
        public string Description { get; set; } = String.Empty;
        public DateTime DateTimeCreate { get; set; }
    }
}
