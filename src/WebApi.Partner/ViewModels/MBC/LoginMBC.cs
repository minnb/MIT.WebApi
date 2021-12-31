namespace VCM.Partner.API.ViewModels.MBC
{
    public class LoginMBC
    {
        public string username { get; set; }
        public string password { get; set; }
        public string merchantId { get; set; }
    }
    public class RspUserLoginMBC
    {
        public string username { get; set; }
        public string accountId { get; set; }
    }
    public class UserMBC: RspUserLoginMBC
    {
        public string jwtToken { get; set; }
    }
    public class RspLoginMBC
    {
        public string code { get; set; }
        public string message { get; set; }
        public object wsResponse { get; set; }
    }
}
