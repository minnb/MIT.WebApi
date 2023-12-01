using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.GCP
{
    public class SurveyResult
    {
        public string AnswerCode { get; set; }
        public string QuestionCode { get; set; }
        public string PhoneNumber { get; set; }
        public string OrderNo { get; set; }
        public string Type { get; set; }
        public int Id { get; set; }
        public int RefID { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
