namespace WebDavFilesRepository.Server.Services
{
    public interface ILogsService
    {
        /// <summary>
        /// добавить лог
        /// </summary>
        /// <param name="logEvent">лог событие</param>
        /// <param name="description">описание</param>
        Task Add(string logEvent, string description);
    }
}
