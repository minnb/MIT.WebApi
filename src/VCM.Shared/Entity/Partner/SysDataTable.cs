using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Partner
{
    [Table("Sys_DataTable")]
    public class SysDataTable
    {
        public string AppCode { get; set; }
        public string TableName { get; set; }
        public int LastCounter { get; set; }
        public int MaxCounter { get; set; }
        public DateTime ChgeDate { get; set; }
        public bool Blocked { get; set; }
        public string StoreProcedures { get; set; }
        public int OrderBy { get; set; }
        public string GroupName { get; set; }
        public string ColumnFilter { get; set; }
        public string Prefix { get; set; }
    }
}
