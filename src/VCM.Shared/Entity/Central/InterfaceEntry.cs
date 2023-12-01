using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    [Table("InterfaceEntry")]
    public class InterfaceEntry
    {
        public string AppCode { get; set; }
        public string JobName { get; set; }
        public string JobType { get; set; }
        public int SortOrder { get; set; }
        public string LocalPath { get; set; }
        public string LocalPathArchived { get; set; }
        public string SftpPath { get; set; }
        public string SftpPathArchived { get; set; }
        public string SftpHost { get; set; }
        public string SftpUser { get; set; }
        public string SftpPass { get; set; }
        public string StoreProName { get; set; }
        public int MaxFile { get; set; }
        public int SfptPermissions { get; set; }
        public string FileExtension { get; set; }
        public string SftpOS { get; set; }
        public string Scheduler { get; set; }
        public int SftpPort { get; set; }
        public bool IsMoveFile { get; set; }
        public string Prefix { get; set; }
        public bool Blocked { get; set; }
        public DateTime CrtDate { get; set; }
        public bool IsMultiThread { get; set; }
        public int PageSize { get; set; }
    }
}
