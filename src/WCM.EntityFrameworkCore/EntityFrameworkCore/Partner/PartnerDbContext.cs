using Microsoft.EntityFrameworkCore;
using VCM.Shared.Entity.Crownx;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Entity.Partner.Shopee;
using VCM.Shared.Entity.SalesPartner;

namespace WCM.EntityFrameworkCore.EntityFrameworkCore.Partner
{
    public class PartnerDbContext : DbContext
    {
        public PartnerDbContext(DbContextOptions<PartnerDbContext> options) : base(options) { }
        public DbSet<User> User { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<SysWebApi> SysWebApi { get; set; }
        public DbSet<SysWebRoute> SysWebRoute { get; set; }
        public DbSet<SysConfig> SysConfig { get; set; }
        public DbSet<TransRaw> TransRaw { get; set; }
        public DbSet<RawData> RawData { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<DataTest> DataTest { get; set; }
        public DbSet<TenderTypeSetup> TenderTypeSetup { get; set; }
        public DbSet<SalesReturnWebOnline> SalesReturnWebOnline { get; set; }
        public DbSet<StoreAndKios> StoreAndKios { get; set; }
        public DbSet<Shopee_update_order> Shopee_update_order { get; set; }
        public DbSet<Shopee_export_error_menu> Shopee_export_error_menu { get; set; }
        public DbSet<Shopee_update_drivers_status> Shopee_update_drivers_status { get; set; }
        public DbSet<Shopee_update_menu> Shopee_update_menu { get; set; }
        public DbSet<ShopeeRestaurant> ShopeeRestaurant { get; set; }
        public DbSet<VoucherIssueInfo> VoucherIssueInfo { get; set; }
        public DbSet<VoucherIssueDetail> VoucherIssueDetail { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VoucherIssueDetail>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });
            modelBuilder.Entity<VoucherIssueInfo>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });
            modelBuilder.Entity<TenderTypeSetup>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.TenderType, e.PartnerCode });
            });

            modelBuilder.Entity<SalesReturnWebOnline>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.InvoiceNo });
            });

            modelBuilder.Entity<DataTest>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.ItemNo });
            });

            modelBuilder.Entity<ShopeeRestaurant>(entity =>
            {
                entity.HasKey(e => new { e.restaurant_id, e.partner_restaurant_id });
            });

            modelBuilder.Entity<Shopee_update_order>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });
            modelBuilder.Entity<Shopee_export_error_menu>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });
            modelBuilder.Entity<Shopee_update_drivers_status>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });
            modelBuilder.Entity<Shopee_update_menu>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });
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
                entity.HasKey(e => new {e.PartnerCode, e.AppCode, e.StoreNo, e.OrderNo });
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
        }
    }
}

