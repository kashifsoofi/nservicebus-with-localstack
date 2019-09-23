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
            // endpointConfiguration.DoNotCreateQueues();

            var serverName = "localhost";
            var transport = endpointConfiguration.UseTransport<SqsTransport>();
            transport.ClientFactory(() => new AmazonSQSClient(
                new AnonymousAWSCredentials(),
                new AmazonSQSConfig
                {
                    ServiceURL = $"http://{serverName}:4576"
                }));

            var s3Configuration = transport.S3("bucketname", "Samples-FullDuplex-Server");
            s3Configuration.ClientFactory(() => new AmazonS3Client(
                new AnonymousAWSCredentials(),
                new AmazonS3Config
                {
                    ServiceURL = $"http://{serverName}:4572",
                    ForcePathStyle = true,
                }));

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            return endpointConfiguration;
        }
    }
}
