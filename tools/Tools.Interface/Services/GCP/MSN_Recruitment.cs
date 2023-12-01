using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Services.GCP
{
    public class MSN_Recruitment
    {
        public long Id { get; set; }
        public string Year { get; set; }
        public string BU { get; set; }
        public string Entity { get; set; }
        public string DirectIndirect { get; set; }
        public string Function { get; set; }
        public string APNumber { get; set; }
        public string Department { get; set; }
        public string Title { get; set; }
        public string DateOfRequest { get; set; }
        public string ExpectedOnboardMonth { get; set; }
        public string DateOfReplace { get; set; }
        public string Reason { get; set; }
        public string NumberOfHire { get; set; }
        public string Rank { get; set; }
        public string Status { get; set; }
        public string ReasonIfClosed { get; set; }
        public string RecruitmentSources { get; set; }
        public string SubRecruitmentSources { get; set; }
        public string ClosingDate { get; set; }
        public string OnboardingDate { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string TA { get; set; }
        public string HRBP { get; set; }
        public string Probation { get; set; }
        public string LeavingDateIfFailProbation { get; set; }
        public string ReasonLeaving { get; set; }
        public string ExitInterviewYesNo { get; set; }
        public int? ProcessingMonth { get; set; }
        public int? ProcessingYear { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FileName { get; set; }
    }
}
