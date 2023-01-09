using Microsoft.EntityFrameworkCore;
using WebDavFilesRepository.Server.Services;
using WebDavFilesRepository.Shared.Entitys;

namespace WebDavFilesRepository.Server.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<FolderAccessEntity> FoldersAccesses { get; set; }
        public DbSet<LogEntity> Logs { get; set; }


        private readonly string connectionString;
        public AppDbContext(IConnectionString connectionString)
        {
            this.connectionString = connectionString.ConnectionString;

            //Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}