using WebDavFilesRepository.Server.Database;
using WebDavFilesRepository.Shared.Entitys;

namespace WebDavFilesRepository.Server.Services
{
    public class LogsService : ILogsService
    {
        public LogsService(AppDbContext dbContext)
        {
            db = dbContext;
        }

        private readonly AppDbContext db;


        public async Task Add(string logEvent, string description)
        {
            LogEntity folderAccess = new()
            {
                LogEvent = logEvent,
                Description = description,
                DateTimeCreate = DateTime.Now
            };

            db.Logs.Add(folderAccess);

            await db.SaveChangesAsync();
        }
    }
}
