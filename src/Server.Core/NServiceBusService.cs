namespace Server.Core
{
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.SimpleNotificationService;
    using Amazon.SQS;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using NServiceBus;
    using Shared.Core.Config;

    public class NServiceBusService : IHostedService
    {
        private readonly NServiceBusOptions nServiceBusOptions;
        private IEndpointInstance endpointInstance;

        public IMessageSession MessageSession { get; internal set; }

        public NServiceBusService(IOptions<NServiceBusOptions> nServiceBusOptions)
        {
            this.nServiceBusOptions = nServiceBusOptions.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var endpointConfiguration = ConfigureEndpoint();

            endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            MessageSession = endpointInstance;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (endpointInstance != null)
            {
                await endpointInstance.Stop().ConfigureAwait(false);
            }
        }

        private EndpointConfiguration ConfigureEndpoint()
        {
            var endpointConfiguration = new EndpointConfiguration("Samples.FullDuplex.Server");
            endpointConfiguration.DoNotCreateQueues();

            var amazonSqsConfig = new AmazonSQSConfig();
            if (!string.IsNullOrEmpty(this.nServiceBusOptions.AwsEndpoint))
            {
                amazonSqsConfig.ServiceURL = this.nServiceBusOptions.AwsEndpoint;
            }

            var transport = endpointConfiguration.UseTransport<SqsTransport>();
            transport.ClientFactory(() => new AmazonSQSClient(
                new AnonymousAWSCredentials(),
                amazonSqsConfig));

            var amazonSimpleNotificationServiceConfig = new AmazonSimpleNotificationServiceConfig();
            if (!string.IsNullOrEmpty(this.nServiceBusOptions.AwsEndpoint))
            {
                amazonSimpleNotificationServiceConfig.ServiceURL = this.nServiceBusOptions.AwsEndpoint;
            }
            
            transport.ClientFactory(() => new AmazonSimpleNotificationServiceClient(
                new AnonymousAWSCredentials(),
                amazonSimpleNotificationServiceConfig));

            var amazonS3Config = new AmazonS3Config
            {
                ForcePathStyle = true,
            };
            if (!string.IsNullOrEmpty(this.nServiceBusOptions.AwsEndpoint))
            {
                amazonS3Config.ServiceURL = this.nServiceBusOptions.AwsEndpoint;
            }

            var s3Configuration = transport.S3("bucketname", "Samples-FullDuplex-Client");
            s3Configuration.ClientFactory(() => new AmazonS3Client(
                new AnonymousAWSCredentials(),
                amazonS3Config));

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();

            return endpointConfiguration;
        }
    }
}
