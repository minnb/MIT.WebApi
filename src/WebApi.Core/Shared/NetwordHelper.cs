namespace WebApi.Core.Shared
{
    public static class NetwordHelper
    {
        public static string GetHostName()
        {
            try
            {
                return System.Net.Dns.GetHostName() ?? "WebApiPartner";
            }
            catch
            {
                return "WebApiPartner";
            }
        }
    }
}
