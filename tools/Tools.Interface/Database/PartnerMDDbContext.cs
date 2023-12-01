using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Entity.SalesPartner;

namespace Tools.Interface.Database
{
    public class PartnerMDDbContext : DbContext
    {
        private string _dbConnectString;
        public PartnerMDDbContext(string dbConnectString)
        {
            _dbConnectString = dbConnectString;
            Database.SetCommandTimeout(150000);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_dbConnectString);
        }

        public DbSet<SysWebApi> SysWebApi { get; set; }
        public DbSet<SysWebRoute> SysWebRoute { get; set; }
        public DbSet<ItemSalesOnApp> ItemSalesOnApp { get; set; }
        public DbSet<ItemToppingMapping> ItemToppingMapping { get; set; }
        public DbSet<ToppingSalesOnApp> ToppingSalesOnApp { get; set; }
        public DbSet<ActionLogging> ActionLogging { get; set; }
        public DbSet<MappingDataPartner> MappingDataPartner { get; set; }
        //==========
        public DbSet<ShopeeRestaurant> ShopeeRestaurant { get; set; }
        public DbSet<ShopeeDish> ShopeeDish { get; set; }
        public DbSet<ShopeeDishGroup> ShopeeDishGroup { get; set; }
        public DbSet<ShopeeDishGroupDetail> ShopeeDishGroupDetail { get; set; }

        //topping
        public DbSet<ShopeeTopping> ShopeeTopping { get; set; }
        public DbSet<ShopeeToppingGroup> ShopeeToppingGroup { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MappingDataPartner>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });
            modelBuilder.Entity<ActionLogging>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });
            modelBuilder.Entity<ItemToppingMapping>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.PartnerCode, e.ItemNo, e.StoreNo, e.ToppingNo });
            });

            modelBuilder.Entity<ToppingSalesOnApp>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.PartnerCode, e.ToppingNo, e.StoreNo });
            });

            modelBuilder.Entity<ItemSalesOnApp>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.PartnerCode, e.ItemNo, e.StoreNo });
            });

            modelBuilder.Entity<ShopeeToppingGroup>(entity =>
            {
                entity.HasKey(e => new { e.topping_group_id, e.partner_restaurant_id });
            });

            modelBuilder.Entity<ShopeeTopping>(entity =>
            {
                entity.HasKey(e => new { e.topping_id, e.partner_restaurant_id });
            });

            modelBuilder.Entity<ShopeeDish>(entity =>
            {
                entity.HasKey(e => new { e.dish_id, e.partner_restaurant_id });
            });

            modelBuilder.Entity<ShopeeDishGroup>(entity =>
            {
                entity.HasKey(e => new { e.dish_group_id, e.partner_restaurant_id });
            });

            modelBuilder.Entity<ShopeeDishGroupDetail>(entity =>
            {
                entity.HasKey(e => new { e.dish_group_id, e.restaurant_id });
            });

            modelBuilder.Entity<ShopeeRestaurant>(entity =>
            {
                entity.HasKey(e => new { e.partner_restaurant_id });
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


