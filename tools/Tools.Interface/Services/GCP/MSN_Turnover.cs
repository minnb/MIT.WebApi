using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Services.GCP
{
    public class MSN_Turnover
    {
        public long Id { get; set; }
        public string Year { get; set; }
        public string EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string DOB { get; set; }
        public string Age { get; set; }
        public string Entity { get; set; }
        public string Department { get; set; }
        public string Onboarddate { get; set; }
        public string Title { get; set; }
        public string Rank { get; set; }
        public string Function { get; set; }
        public string Stafftype { get; set; }
        public string Onboard { get; set; }
        public string OnboardOfficial { get; set; }
        public string EffectiveOff { get; set; }
        public string ContractType { get; set; }
        public string NumberOfWorkingYears { get; set; }
        public string NumberOfWorkingMonths { get; set; }
        public string LocalExpat { get; set; }
        public string TypeOfLeaving { get; set; }
        public string F9box { get; set; }
        public string DirectIndirect { get; set; }
        public string CountTurnover { get; set; }
        public string BU { get; set; }
        public string VoluntaryInvoluntary { get; set; }
        public int? ProcessingMonth { get; set; }
        public int? ProcessingYear { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FileName { get; set; }
    }
}
