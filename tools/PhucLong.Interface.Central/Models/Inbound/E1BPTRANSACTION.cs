using System.Xml.Serialization;

namespace PhucLong.Interface.Central.Models.Inbound
{
    public class E1BPTRANSACTION: INB_HEADER
    {
        public string BEGINDATETIMESTAMP { get; set; }
        public string ENDDATETIMESTAMP { get; set; }
        public string TRANSACTIONCURRENCY { get; set; }
        public string TRANSACTIONCURRENCY_ISO { get; set; }
        public string PARTNERQUALIFIER { get; set; }
        public string PARTNERID { get; set; }
    }
}
