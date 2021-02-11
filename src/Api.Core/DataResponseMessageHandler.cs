namespace Api.Core
{
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Logging;
    using Shared.Core;

    class DataResponseMessageHandler : IHandleMessages<DataResponseMessage>
    {
        static readonly ILog log = LogManager.GetLogger<DataResponseMessageHandler>();
        public Task Handle(DataResponseMessage message, IMessageHandlerContext context)
        {
            log.Info($"Response received with description: {message.String}");
            return Task.CompletedTask;
        }
    }
}
