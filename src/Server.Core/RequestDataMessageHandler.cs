namespace Server.Core
{
    using NServiceBus;
    using System.Threading.Tasks;
    using NServiceBus.Logging;
    using Shared.Core;

    public class RequestDataMessageHandler : IHandleMessages<RequestDataMessage>
    {
        static readonly ILog log = LogManager.GetLogger<RequestDataMessageHandler>();

        public async Task Handle(RequestDataMessage message, IMessageHandlerContext context)
        {
            log.Info($"Received request {message.DataId}.");
            log.Info($"String received: {message.String}.");

            var response = new DataResponseMessage
            {
                DataId = message.DataId,
                String = message.String
            };

            await context.Reply(response)
                .ConfigureAwait(false);
        }
    }
}