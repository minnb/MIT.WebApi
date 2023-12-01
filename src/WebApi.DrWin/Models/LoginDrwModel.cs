namespace WebApi.DrWin.Models
{
    public class LoginDrwModel
    {
        public string Usr { get; set; }
        public string Pwd { get; set; }
    }

    public class LoginRsp
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public TokenDrw Data { get; set; }
    }
    public class TokenDrw
    {
        public string Token { get; set; }
        public string Token_type { get; set; }
    }
    public class Tai_khoan_ket_noi_drw
    {
        public string Ma_sap { get; set; }
        public string Ma_co_so { get; set; }
        public string Tai_khoan_ket_noi { get; set; }
        public string Token { get; set; }
        public string Token_type { get; set; }
    }
}
