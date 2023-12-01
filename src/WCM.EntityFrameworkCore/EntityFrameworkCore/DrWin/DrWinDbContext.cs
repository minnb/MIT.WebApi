using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VCM.Shared.Entity.DrWin;
using VCM.Shared.Entity.Partner;

namespace WCM.EntityFrameworkCore.EntityFrameworkCore.DrWin
{
    public class DrWinDbContext : DbContext
    {
        public DrWinDbContext(DbContextOptions<DrWinDbContext> options) : base(options) { }
        public DbSet<User> User { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<SysWebApi> SysWebApi { get; set; }
        public DbSet<SysWebRoute> SysWebRoute { get; set; }
        public DbSet<SysConfig> SysConfig { get; set; }
        public DbSet<M_tai_khoan_ket_noi> Tai_Khoan_Ket_Noi { get; set; }
        public DbSet<M_hoa_don_thuoc> M_hoa_don_thuoc { get; set; }
        public DbSet<M_hoa_don_chi_tiet> M_hoa_don_chi_tiet { get; set; }
        public DbSet<M_Phieu_xuat_thuoc> M_Phieu_xuat_thuoc { get; set; }
        public DbSet<M_Phieu_xuat_thuoc_chi_tiet> M_Phieu_xuat_thuoc_chi_tiet { get; set; }
        public DbSet<M_Phieu_nhap_thuoc> M_Phieu_nhap_thuoc { get; set; }
        public DbSet<M_Phieu_nhap_thuoc_chi_tiet> M_Phieu_nhap_thuoc_chi_tiet { get; set; }
        public DbSet<M_Logging_Api> M_Logging_Api { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<M_Logging_Api>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });

            modelBuilder.Entity<M_ton_kho>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });

            modelBuilder.Entity<M_Phieu_nhap_thuoc_chi_tiet>(entity =>
            {
                entity.HasKey(e => new { e.Ma_phieu, e.Ma_thuoc });
            });

            modelBuilder.Entity<M_Phieu_nhap_thuoc>(entity =>
            {
                entity.HasKey(e => new { e.Ma_phieu });
            });

            modelBuilder.Entity<M_Phieu_xuat_thuoc_chi_tiet>(entity =>
            {
                entity.HasKey(e => new { e.Ma_phieu, e.Ma_thuoc });
            });

            modelBuilder.Entity<M_Phieu_xuat_thuoc>(entity =>
            {
                entity.HasKey(e => new { e.Ma_phieu });
            });

            modelBuilder.Entity<M_hoa_don_chi_tiet>(entity =>
            {
                entity.HasKey(e => new { e.Ma_hoa_don, e.Ma_thuoc });
            });

            modelBuilder.Entity<M_hoa_don_thuoc>(entity =>
            {
                entity.HasKey(e => new { e.Ma_hoa_don });
            });

            modelBuilder.Entity<M_tai_khoan_ket_noi>(entity =>
            {
                entity.HasKey(e => new { e.Ma_sap});
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
