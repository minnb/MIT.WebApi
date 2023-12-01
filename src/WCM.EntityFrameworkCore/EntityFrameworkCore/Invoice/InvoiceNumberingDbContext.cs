using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VCM.Shared.Entity.Invoice;
using VCM.Shared.Entity.Partner;

namespace WCM.EntityFrameworkCore.EntityFrameworkCore.Invoice
{
    public class InvoiceNumberingDbContext : DbContext
    {
        public InvoiceNumberingDbContext(DbContextOptions<InvoiceNumberingDbContext> options) : base(options) { }
        public DbSet<InvoiceNumbering> InvoiceNumbering { get; set; }
        public DbSet<InvoiceNumberingDetail> InvoiceNumberingDetail { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InvoiceNumbering>(entity =>
            {
                entity.HasKey(e => new { e.TaxCode, e.TemplateNo, e.SerialNo, e.StartDate });
            });
            modelBuilder.Entity<InvoiceNumberingDetail>(entity =>
            {
                entity.HasKey(e => new { e.StoreNo, e.PosNo, e.TemplateNo, e.SerialNo, e.InvoiceNumber });
            });
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => new { e.UserName, e.AppCode });
            });
            modelBuilder.Entity<UserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserName, e.RoleName });
            });
        }
    }
}
