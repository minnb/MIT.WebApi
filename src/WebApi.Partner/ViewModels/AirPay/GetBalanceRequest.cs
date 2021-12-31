namespace VCM.Partner.API.ViewModels.AirPay
{
    public class GetBalanceRequest
    {
        public string partner_id { get; set; }
        public string signature { get; set; }
    }
}
