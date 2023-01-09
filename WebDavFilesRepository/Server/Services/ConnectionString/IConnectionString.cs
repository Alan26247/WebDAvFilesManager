namespace WebDavFilesRepository.Server.Services
{
    public interface IConnectionString
    {
        /// <summary>
        /// получить строку подключения
        /// </summary>
        string ConnectionString { get; }
    }
}
