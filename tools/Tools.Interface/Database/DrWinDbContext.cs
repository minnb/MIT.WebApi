using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Tools.Interface.Dtos.DRW;
using VCM.Shared.Entity.DrWin;
using VCM.Shared.Entity.Partner;
using WCM.EntityFrameworkCore.EntityFrameworkCore.DrWin;

namespace Tools.Interface.Database
{
    public class DrWinDbContext : DbContext
    {
        private string _dbConnectString;
        public DrWinDbContext(string dbConnectString)
        {
            _dbConnectString = dbConnectString;
            Database.SetCommandTimeout(150000);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_dbConnectString);
        }

        public DbSet<User> User { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<SysWebApi> SysWebApi { get; set; }
        public DbSet<SysWebRoute> SysWebRoute { get; set; }
        public DbSet<SysConfig> SysConfig { get; set; }
        public DbSet<M_hoa_don_thuoc> M_hoa_don_thuoc { get; set; }
        public DbSet<M_hoa_don_chi_tiet> M_hoa_don_chi_tiet { get; set; }
        public DbSet<M_ton_kho> Ton_kho_drw { get; set; }
        public DbSet<ItemDrwDto> ItemDrwDto { get; set; }
        public DbSet<QueryResultDRW> QueryResultDRW { get; set; }
        public DbSet<M_Logging_Api> M_Logging_Api { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QueryResultDRW>(entity =>
            {
                entity.HasKey(e => new { e.Result });
            });

            modelBuilder.Entity<M_Logging_Api>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });

            modelBuilder.Entity<ItemDrwDto>(entity =>
            {
                entity.HasKey(e => new { e.ItemNo, e.SalesUnit, e.BaseUnit });
            });

            modelBuilder.Entity<M_ton_kho>(entity =>
            {
                entity.HasKey(e => new { e.Ma_sap, e.Ma_thuoc, e.So_lo, e.Don_vi_tinh });
            });

            modelBuilder.Entity<M_hoa_don_chi_tiet>(entity =>
            {
                entity.HasKey(e => new { e.Ma_hoa_don, e.Ma_thuoc });
            });

            modelBuilder.Entity<M_hoa_don_thuoc>(entity =>
            {
                entity.HasKey(e => new { e.Ma_hoa_don });
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

    public class QueryResultDRW
    {
        public string Result { get; set; }
    }
}
