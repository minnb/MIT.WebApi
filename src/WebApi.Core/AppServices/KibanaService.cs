using Microsoft.Extensions.Logging;

namespace WebApi.Core.AppServices
{
    public interface IKibanaService
    {
        void LogRequest(string clientName, string bodyJson);
        void LogResponse(string webApi, string clientName, string message, long timeResponse);
    }
    public class KibanaService : IKibanaService
    {
        private readonly ILogger<IKibanaService> _logger;
        public KibanaService
            (
                ILogger<IKibanaService> logger
            )
        {
            _logger = logger;
        }
        public void LogRequest(string clientName, string bodyJson)
        {
            _logger.LogWarning("{HttpContext} {PosNo} {Message}", "Request", clientName, bodyJson??"");
        }

        public void LogResponse(string webApi, string clientName, string message, long timeResponse)
        {
            _logger.LogWarning("{HttpContext} {WebApi} {PosNo} {Message} {ResponseTime}", "Response", webApi, clientName, message, timeResponse);
        }
    }
}
