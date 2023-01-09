namespace WebDavFilesRepository.Server.Services
{
    public class WebDAVConnectionStringService : IWebDAVConnectionStringService
    {
        public string ConnectionString { get; }
        public WebDAVConnectionStringService(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
