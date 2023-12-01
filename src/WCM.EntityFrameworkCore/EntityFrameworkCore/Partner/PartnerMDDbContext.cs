using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Entity.SalesPartner;

namespace WCM.EntityFrameworkCore.EntityFrameworkCore.Partner
{
    public class PartnerMDDbContext : DbContext
    {
        public PartnerMDDbContext(DbContextOptions<PartnerMDDbContext> options) : base(options) { }
        public DbSet<SysWebApi> SysWebApi { get; set; }
        public DbSet<SysWebRoute> SysWebRoute { get; set; }
        public DbSet<ShopeeRestaurant> ShopeeRestaurant { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShopeeRestaurant>(entity =>
            {
                entity.HasKey(e => new { e.restaurant_id, e.partner_restaurant_id });
            });
            modelBuilder.Entity<SysWebRoute>(entity =>
            {
                entity.HasKey(e => new { e.Name, e.AppCode });
            });
            modelBuilder.Entity<SysWebApi>(entity =>
            {
                entity.HasKey(e => new { e.AppCode });
            });
        }
    }
}
