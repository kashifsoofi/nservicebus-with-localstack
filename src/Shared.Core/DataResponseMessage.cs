namespace Shared.Core
{
    using NServiceBus;
    using System;

    public class DataResponseMessage : IMessage
    {
        public Guid Id { get; set; }
        public Guid DataId { get; set; }
        public string String { get; set; }
    }
}
