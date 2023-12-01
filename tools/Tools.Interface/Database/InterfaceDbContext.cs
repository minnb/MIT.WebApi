using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using VCM.Shared.Entity.Central;

namespace Tools.Interface.Database
{
    public class InterfaceDbContext : DbContext
    {
        private string _dbConnectString;
        public InterfaceDbContext()
        {
            Database.SetCommandTimeout(150000);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();
            optionsBuilder.UseSqlServer(configuration["ConnectionStrings:Default"]);
            _dbConnectString = configuration["ConnectionStrings:Default"];
        }

        public DbSet<InterfaceEntry> InterfaceEntry { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InterfaceEntry>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.JobType, e.JobName, e.SortOrder });
            });
        }
    }
}
