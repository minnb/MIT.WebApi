using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VCM.Shared.Entity.Central;

namespace Tools.Interface.Database
{
    [Table("KIOS_TransHeader")]
    public class KIOS_TransHeader: TransHeader
    {

    }
    [Table("KIOS_TransLine")]
    public class KIOS_TransLine : TransLine
    {

    }
    [Table("KIOS_TransPaymentEntry")]
    public class KIOS_TransPaymentEntry : TransPaymentEntry
    {

    }
    [Table("KIOS_TransDiscountEntry")]
    public class KIOS_TransDiscountEntry : TransDiscountEntry
    {

    }
    public class KiosDbContext: DbContext
    {
        private string _dbConnectString;
        public KiosDbContext(string dbConnectString)
        {
            _dbConnectString = dbConnectString;
            Database.SetCommandTimeout(150000);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_dbConnectString);
        }
        public DbSet<KIOS_TransHeader> KIOS_TransHeader { get; set; }
        public DbSet<KIOS_TransDiscountEntry> KIOS_TransDiscountEntry { get; set; }
        public DbSet<KIOS_TransLine> KIOS_TransLine { get; set; }
        public DbSet<KIOS_TransPaymentEntry> KIOS_TransPaymentEntry { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<KIOS_TransHeader>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.OrderNo});
            });
            modelBuilder.Entity<KIOS_TransLine>(entity =>
            {
                entity.HasKey(e => new { e.LineNo, e.OrderNo });
            });
            modelBuilder.Entity<KIOS_TransPaymentEntry>(entity =>
            {
                entity.HasKey(e => new { e.LineNo, e.OrderNo });
            });
            modelBuilder.Entity<KIOS_TransDiscountEntry>(entity =>
            {
                entity.HasKey(e => new { e.LineNo, e.OrderNo });
            });

        }
    }
}
