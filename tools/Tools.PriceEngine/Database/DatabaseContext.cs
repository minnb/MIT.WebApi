using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VCM.Shared.Entity.Partner;

namespace Tools.PriceEngine.Database
{
    public class DatabaseContext : DbContext
    {
        private string _dbConnectString;
        public DatabaseContext()
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
        public DbSet<SysConfig> SysConfig { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SysConfig>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.Name });
            });
        }
    }
}
