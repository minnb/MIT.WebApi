using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Central.Models.Inbound;
using System.IO;
using VCM.Shared.Entity.Central;

namespace PhucLong.Interface.Central.Database
{
    public class CentralDbContext : DbContext
    {
        public CentralDbContext()
        {
            Database.SetCommandTimeout(150000);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();
            optionsBuilder.UseSqlServer(configuration["ConnectionStrings:PhucLongStaging"]);

        }
        public DbSet<TransHeader> TransHeader { get; set; }
        public DbSet<TransLine> TransLine { get; set; }
        public DbSet<TransPaymentEntry> TransPaymentEntry { get; set; }
        public DbSet<TransDiscountEntry> TransDiscountEntry { get; set; }
        public DbSet<TransInfoVAT> TransInfoVAT { get; set; }
        public DbSet<TransCancel> TransCancel { get; set; }
        public DbSet<TransLineOptions> TransLineOptions { get; set; }
        public DbSet<InterfaceEntry> InterfaceEntry { get; set; }
        public DbSet<MappingChannel> MappingChannel { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MappingChannel>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.SaleTypeId });
            });
            modelBuilder.Entity<TransHeader>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.OrderNo });
            });
            modelBuilder.Entity<TransLine>(entity =>
            {
                entity.HasKey(e => new { e.LineNo, e.OrderNo });
            });
            modelBuilder.Entity<TransPaymentEntry>(entity =>
            {
                entity.HasKey(e => new { e.LineNo, e.OrderNo });
            });
            modelBuilder.Entity<TransDiscountEntry>(entity =>
            {
                entity.HasKey(e => new { e.LineNo, e.OrderNo });
            });
            modelBuilder.Entity<TransInfoVAT>(entity =>
            {
                entity.HasKey(e => new { e.OrderNo });
            });
            modelBuilder.Entity<TransCancel>(entity =>
            {
                entity.HasKey(e => new {e.Id, e.OrderNo });
            });
            modelBuilder.Entity<TransLineOptions>(entity =>
            {
                entity.HasKey(e => new { e.OrderNo, e.LineNo, e.OrderLineNo });
            });

            modelBuilder.Entity<InterfaceEntry>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.JobType, e.JobName, e.SortOrder });
            });

         }
    }
}
