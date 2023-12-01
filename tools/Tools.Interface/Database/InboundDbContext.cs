using Microsoft.EntityFrameworkCore;
using PhucLong.Interface.Central.Models.Master;
using PhucLong.Interface.Central.Models.OCC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Database
{
    public class InboundDbContext : DbContext
    {
        private string _dbConnectString;
        public InboundDbContext(string dbConnectString)
        {
            _dbConnectString = dbConnectString;
            Database.SetCommandTimeout(150000);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_dbConnectString);
        }
        public DbSet<OCCTransLine> OCCTransLine { get; set; }
        public DbSet<OCCTransHeader> OCCTransHeader { get; set; }
        public DbSet<OCCTransPaymentEntry> OCCTransPaymentEntry { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OCCTransHeader>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.OrderNo });
            });
            modelBuilder.Entity<OCCTransLine>(entity =>
            {
                entity.HasKey(e => new { e.LineNo, e.OrderNo });
            });
            modelBuilder.Entity<OCCTransPaymentEntry>(entity =>
            {
                entity.HasKey(e => new { e.LineNo, e.OrderNo });
            });

        }
    }
}
