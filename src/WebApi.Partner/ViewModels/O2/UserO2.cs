namespace WebApi.Partner.ViewModels.O2
{
    public class UserO2
    {
        public string Phone_number { get; set; }
        public string Tier { get; set; } //POTENTIAL, VIP, NORMAL
        public bool Potential_vip { get; set; }
        public bool Evo { get; set; }
        public bool Tpay { get; set; }
        public bool Wintel { get; set; }
        public int QuotaMoney { get; set; }

    }
}
