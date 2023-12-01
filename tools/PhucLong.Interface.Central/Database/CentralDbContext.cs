using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Central.Models.Inbound;
using PhucLong.Interface.Central.Models.Master;
using PhucLong.Interface.Central.Models.OCC;
using System;
using System.Collections;
using System.Data;
using System.IO;
using VCM.Common.Helpers;
using VCM.Shared.Entity.Central;

namespace PhucLong.Interface.Central.Database
{
    public class CentralDbContext : DbContext
    {
        private string _dbConnectString;
        public CentralDbContext()
        {
            Database.SetCommandTimeout(150000);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();
            optionsBuilder.UseSqlServer(configuration["ConnectionStrings:PhucLongStaging"]);
            _dbConnectString = configuration["ConnectionStrings:PhucLongStaging"];

        }
        public DbSet<TransHeader> TransHeader { get; set; }
        public DbSet<TransLine> TransLine { get; set; }
        public DbSet<TransPaymentEntry> TransPaymentEntry { get; set; }
        public DbSet<TransDiscountEntry> TransDiscountEntry { get; set; }
        public DbSet<TransInfoVAT> TransInfoVAT { get; set; }
        public DbSet<TransCancel> TransCancel { get; set; }
        public DbSet<TransLineOptions> TransLineOptions { get; set; }
        public DbSet<InterfaceEntry> InterfaceEntry { get; set; }
        public DbSet<InterfaceSetup> InterfaceSetup { get; set; }
        public DbSet<MappingChannel> MappingChannel { get; set; }
        public DbSet<MappingStore> MappingStore { get; set; }
        public DbSet<MappingTender> MappingTender { get; set; }
        public DbSet<MappingVAT> MappingVAT { get; set; }
        public DbSet<Jobs> Jobs { get; set; }
        public DbSet<Logging> Logging { get; set; }
        public DbSet<Temp_Item> Temp_Item { get; set; }
        public DbSet<MasterItem> MasterItem { get; set; }
        public DbSet<OCCTransLine> OCCTransLine { get; set; }
        public DbSet<OCCTransHeader> OCCTransHeader { get; set; }
        public DbSet<OCCTransPaymentEntry> OCCTransPaymentEntry { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MasterItem>(entity =>
            {
                entity.HasKey(e => new { e.ItemNo, e.Uom });
            });
            modelBuilder.Entity<MappingTender>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.Type, e.WCM, e.TenderType });
            });
            modelBuilder.Entity<MappingStore>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.TenderType, e.StoreNo, e.StoreNo2 });
            });
            modelBuilder.Entity<OCCTransPaymentEntry>(entity =>
            {
                entity.HasKey(e => new { e.OrderNo, e.LineNo });
            });
            modelBuilder.Entity<OCCTransLine>(entity =>
            {
                entity.HasKey(e => new { e.OrderNo, e.LineNo });
            });

            modelBuilder.Entity<OCCTransHeader>(entity =>
            {
                entity.HasKey(e => new { e.OrderNo });
            });

            modelBuilder.Entity<Temp_Item>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });

            modelBuilder.Entity<Logging>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });

            modelBuilder.Entity<MappingVAT>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.Code });
            });

            modelBuilder.Entity<Jobs>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });

            modelBuilder.Entity<MappingChannel>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.SaleTypeId });
            });
            modelBuilder.Entity<TransHeader>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.OrderNo });
            });
            modelBuilder.Entity<TransLine>(entity =>
            {
                entity.HasKey(e => new { e.LineNo, e.OrderNo });
            });
            modelBuilder.Entity<TransPaymentEntry>(entity =>
            {
                entity.HasKey(e => new { e.LineNo, e.OrderNo });
            });
            modelBuilder.Entity<TransDiscountEntry>(entity =>
            {
                entity.HasKey(e => new { e.LineNo, e.OrderNo });
            });
            modelBuilder.Entity<TransInfoVAT>(entity =>
            {
                entity.HasKey(e => new { e.OrderNo });
            });
            modelBuilder.Entity<TransCancel>(entity =>
            {
                entity.HasKey(e => new {e.Id, e.OrderNo });
            });
            modelBuilder.Entity<TransLineOptions>(entity =>
            {
                entity.HasKey(e => new { e.OrderNo, e.LineNo, e.OrderLineNo });
            });

            modelBuilder.Entity<InterfaceEntry>(entity =>
            {
                entity.HasKey(e => new { e.AppCode, e.JobType, e.JobName, e.SortOrder });
            });
            modelBuilder.Entity<InterfaceSetup>(entity =>
            {
                entity.HasKey(e => new {  e.JobName, e.Task, e.Sort });
            });

        }
        public DataSet ExecuteProcedure(string procName, Hashtable parms)
        {
            DataSet ds = new DataSet("ExecuteProcedure");
            using (SqlConnection connection = new SqlConnection(_dbConnectString))
            {
                SqlCommand cmd = new SqlCommand
                {
                    CommandText = procName,
                    CommandType = CommandType.StoredProcedure
                };
                using (var da = new SqlDataAdapter(cmd.CommandText, connection))
                {
                    try
                    {
                        cmd.Connection = connection;
                        if (parms.Count > 0)
                        {
                            foreach (DictionaryEntry deparams in parms)
                            {
                                cmd.Parameters.AddWithValue(deparams.Key.ToString(), deparams.Value);
                            }
                        }
                        da.SelectCommand = cmd;
                        da.Fill(ds);
                    }
                    catch (Exception ex)
                    {
                        FileHelper.WriteLogs("CentralDbContext.ExecuteProcedure Exception: " + ex.ToString());
                    }
                }
            }
            return ds;
        }
    }
}
