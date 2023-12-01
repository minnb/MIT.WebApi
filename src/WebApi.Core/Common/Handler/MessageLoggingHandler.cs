using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Core.Common.Handler
{
    public class MessageLoggingHandler : MessageHandler
    {
        private readonly ILogger<MessageLoggingHandler> _logger = null;
        public MessageLoggingHandler(ILogger<MessageLoggingHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        protected override async Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() => _logger.LogWarning(string.Format("{0} - Request: {1}\r\n{2}", correlationId, requestInfo, Encoding.UTF8.GetString(message))));
        }

        protected override async Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() => _logger.LogWarning(string.Format("{0} - Response: {1}\r\n{2}", correlationId, requestInfo, Encoding.UTF8.GetString(message))));
        }
    }
}
