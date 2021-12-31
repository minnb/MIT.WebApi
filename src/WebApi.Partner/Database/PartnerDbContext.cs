using Microsoft.EntityFrameworkCore;
using VCM.Shared.Entity.Partner;

namespace VCM.Partner.API.Database
{
    public class PartnerDbContext : DbContext
    {
        public PartnerDbContext(DbContextOptions<PartnerDbContext> options) : base(options) { }
        public DbSet<User> User { get; set; }
        public DbSet<SysWebApi> SysWebApi { get; set; }
        public DbSet<SysWebRoute> SysWebRoute { get; set; }
        public DbSet<TransRaw> TransRaw { get; set; }
        public DbSet<RawData> RawData { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<StoreAndKios> StoreAndKios { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StoreAndKios>(entity =>
            {
                entity.HasKey(e => new { e.LocationId, e.StoreNo, e.WarehouseId });
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.ItemNo });
            });

            modelBuilder.Entity<TransRaw>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });

            modelBuilder.Entity<RawData>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => new { e.UserName, e.AppCode });
            });
            modelBuilder.Entity<SysWebApi>(entity =>
            {
                entity.HasKey(e => new { e.AppCode });
            });
            modelBuilder.Entity<SysWebRoute>(entity =>
            {
                entity.HasKey(e => new { e.Name, e.AppCode });
            });
        }
    }
}
