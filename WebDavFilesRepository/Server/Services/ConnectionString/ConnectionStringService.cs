namespace WebDavFilesRepository.Server.Services
{
    public class ConnectionStringService : IConnectionString
    {
        public string ConnectionString { get; }
        public ConnectionStringService(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
