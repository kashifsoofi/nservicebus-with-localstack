namespace Server.Core
{
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.SQS;
    using Microsoft.Extensions.Hosting;
    using NServiceBus;

    public class NServiceBusService : IHostedService
    {
        private IEndpointInstance endpointInstance;

        public IMessageSession MessageSession { get; internal set; }

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

            var transport = endpointConfiguration.UseTransport<SqsTransport>();
            transport.ClientFactory(() => new AmazonSQSClient(
                new AnonymousAWSCredentials(),
                new AmazonSQSConfig
                {
                    ServiceURL = "http://localstack:4576"
                }));

            var s3Configuration = transport.S3("bucketname", "my/key/prefix");
            s3Configuration.ClientFactory(() => new AmazonS3Client(
                new AnonymousAWSCredentials(),
                new AmazonS3Config
                {
                    ServiceURL = "http://localstack:4572"
                }));

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            return endpointConfiguration;
        }
    }
}
