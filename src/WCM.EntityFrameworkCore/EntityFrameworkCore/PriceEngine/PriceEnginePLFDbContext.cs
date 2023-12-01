using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VCM.Shared.Entity.PriceEngine;
using WCM.EntityFrameworkCore.Dtos;

namespace WCM.EntityFrameworkCore.EntityFrameworkCore.PriceEngine
{
    public class PriceEnginePLFDbContext : DbContext
    {
        public PriceEnginePLFDbContext
            (DbContextOptions<PriceEnginePLFDbContext> options) : base(options)
        {
        }
        public DbSet<TmpTransHeader> TmpTransHeader { get; set; }
        public DbSet<TmpTransLine> TmpTransLine { get; set; }
        public DbSet<TmpTransDiscount> TmpTransDiscount { get; set; }
        public DbSet<QueryResults> QueryResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                entity.HasKey(e => new { e.OrderNo, e.LineNo, e.OrderLineNo });
            });
        }
    }
}
