using Microsoft.EntityFrameworkCore;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Entity.PriceEngine;
using WCM.EntityFrameworkCore.Dtos;

namespace WCM.EntityFrameworkCore.EntityFrameworkCore.PriceEngine
{
    public class PriceEngineDbContext : DbContext
    {
        public PriceEngineDbContext(DbContextOptions<PriceEngineDbContext> options) : base(options) { }
        public DbSet<User> User { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<SysWebApi> SysWebApi { get; set; }
        public DbSet<SysWebRoute> SysWebRoute { get; set; }
        public DbSet<SysConfig> SysConfig { get; set; }
        public DbSet<SysDataTable> SysDataTable { get; set; }
        public DbSet<TmpTransHeader> TmpTransHeader { get; set; }
        public DbSet<TmpTransLine> TmpTransLine { get; set; }
        public DbSet<TmpTransDiscount> TmpTransDiscount { get; set; }
        public DbSet<TmpTransRaw> TmpTransRaw { get; set; }
        public DbSet<QueryResults> QueryResults { get; set; }
        public DbSet<SysStoreSet> SysStoreSet { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SysStoreSet>(entity =>
            {
                entity.HasKey(e => new { e.StoreNo, e.SubSet });
            });

            modelBuilder.Entity<TmpTransRaw>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.StoreNo, e.OrderNo });
            });

            modelBuilder.Entity<QueryResults>(entity =>
            {
                entity.HasKey(e => new { e.Results });
            });

            modelBuilder.Entity<TmpTransHeader>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.OrderNo, e.StoreNo });
            });
            modelBuilder.Entity<TmpTransLine>(entity =>
            {
                entity.HasKey(e => new { e.OrderNo, e.LineNo });
            });
            modelBuilder.Entity<TmpTransDiscount>(entity =>
            {
                entity.HasKey(e => new { e.OrderNo, e.LineNo });
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => new { e.UserName, e.AppCode });
            });
            modelBuilder.Entity<UserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserName, e.RoleName });
            });
            modelBuilder.Entity<SysWebApi>(entity =>
            {
                entity.HasKey(e => new { e.AppCode });
            });
            modelBuilder.Entity<SysWebRoute>(entity =>
            {
                entity.HasKey(e => new { e.Name, e.AppCode });
            });
            modelBuilder.Entity<SysConfig>(entity =>
            {
                entity.HasKey(e => new { e.Name, e.AppCode });
            });
            modelBuilder.Entity<SysDataTable>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.TableName });
            });
        }
    }
    
}
