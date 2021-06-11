using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using MIT.Dtos;
using System.IO;

namespace MIT.EntityFrameworkCore
{
    public class GPAYDbContext : DbContext
    {
       protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();
            optionsBuilder.UseSqlServer(configuration["ConnectionStrings:Default"]);
        }

        public DbSet<UserDto> UserDto { get; set; }
        public DbSet<WebApiDto> WebApiDto { get; set; }
        public DbSet<WebRouteDto> WebRouteDto { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserDto>(entity =>
            {
                entity.HasKey(e => new { e.UserName, e.AppCode });
            });
            modelBuilder.Entity<WebApiDto>(entity =>
            {
                entity.HasKey(e => new { e.AppCode });
            });
            modelBuilder.Entity<WebRouteDto>(entity =>
            {
                entity.HasKey(e => new { e.Name, e.AppCode });
            });
        }
    }
}
