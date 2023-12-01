using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Services.GCP
{
    public class MSN_Training
    {
        public long Id { get; set; }
        public string Year { get; set; }
        public string BU { get; set; }
        public string Entity { get; set; }
        public string Function { get; set; }
        public string EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Rank { get; set; }
        public string Position { get; set; }
        public string LineManager { get; set; }
        public string HRBP { get; set; }
        public string CourseID { get; set; }
        public string Course { get; set; }
        public string Organizer { get; set; }
        public string Type { get; set; }
        public string Supplier { get; set; }
        public string Hour { get; set; }
        public string TrainingCost { get; set; }
        public string DirectIndirect { get; set; }
        public int? ProcessingMonth { get; set; }
        public int? ProcessingYear { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FileName { get; set; }
    }
}
