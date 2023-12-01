using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VCM.Shared.Entity.Central
{
    [Table("InterfaceSetup")]
    public class InterfaceSetup
    {
        public string JobName { get; set; }
        public string Task { get; set; }
        public int Sort { get; set; }
        public string StoreProcedure { get; set; }
        public string LocalPath { get; set; }
        public string LocalPathArchived { get; set; }
        public string SftpPath { get; set; }
        public string SftpHost { get; set; }
        public string SftpUser { get; set; }
        public string SftpPass { get; set; }
        public string SftpOS { get; set; }
        public int SftpPort { get; set; }
        public int MaxFile { get; set; }
        public int SfptPermissions { get; set; }
        public string FileExtension { get; set; }
        public string Prefix { get; set; }
        public bool IsMoveFile { get; set; }
        public bool Blocked { get; set; }
        public string Description { get; set; }
    }
}
