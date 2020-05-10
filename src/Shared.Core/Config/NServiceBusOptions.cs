namespace Shared.Core.Config
{
    public class NServiceBusOptions
    {
        public string SqsServiceUrlOverride { get; set; }

        public string S3ServiceUrlOverride { get; set; }

        public string SnsServiceUrlOverride { get; set; }
    }
}
