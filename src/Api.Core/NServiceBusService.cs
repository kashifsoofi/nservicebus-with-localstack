﻿namespace Api.Core
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.SimpleNotificationService;
    using Amazon.SQS;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using NServiceBus;
    using Shared.Core;
    using Shared.Core.Config;

    public class NServiceBusService : IHostedService
    {
        private readonly NServiceBusOptions nServiceBusOptions;
        private IEndpointInstance endpointInstance;

        public IMessageSession MessageSession { get; internal set; }
        public ExceptionDispatchInfo StartupException { get; internal set; }

        public NServiceBusService(IOptions<NServiceBusOptions> nServiceBusOptions)
        {
            this.nServiceBusOptions = nServiceBusOptions.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var endpointConfiguration = ConfigureEndpoint();

            try
            {
                endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
                MessageSession = endpointInstance;
            }
            catch (Exception e)
            {
                StartupException = ExceptionDispatchInfo.Capture(e);
            }
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
            var endpointConfiguration = new EndpointConfiguration("Samples.FullDuplex.Client");
            endpointConfiguration.DoNotCreateQueues();

            var amazonSqsConfig = new AmazonSQSConfig();
            if (!string.IsNullOrEmpty(nServiceBusOptions.SqsServiceUrlOverride))
            {
                amazonSqsConfig.ServiceURL = nServiceBusOptions.SqsServiceUrlOverride;
            }

            var transport = endpointConfiguration.UseTransport<SqsTransport>();
            transport.ClientFactory(() => new AmazonSQSClient(
                new AnonymousAWSCredentials(),
                amazonSqsConfig));

            var amazonSimpleNotificationServiceConfig = new AmazonSimpleNotificationServiceConfig();
            if (!string.IsNullOrEmpty(nServiceBusOptions.SnsServiceUrlOverride))
            {
                amazonSimpleNotificationServiceConfig.ServiceURL = nServiceBusOptions.SnsServiceUrlOverride;
            }

            transport.ClientFactory(() => new AmazonSimpleNotificationServiceClient(
                new AnonymousAWSCredentials(),
                amazonSimpleNotificationServiceConfig));

            var amazonS3Config = new AmazonS3Config
            {
                ForcePathStyle = true,
            };
            if (!string.IsNullOrEmpty(nServiceBusOptions.S3ServiceUrlOverride))
            {
                amazonS3Config.ServiceURL = nServiceBusOptions.S3ServiceUrlOverride;
            }

            var s3Configuration = transport.S3("bucketname", "Samples-FullDuplex-Client");
            s3Configuration.ClientFactory(() => new AmazonS3Client(
                new AnonymousAWSCredentials(),
                amazonS3Config));

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(RequestDataMessage), "Samples.FullDuplex.Server");

            endpointConfiguration.EnableCallbacks();
            endpointConfiguration.MakeInstanceUniquelyAddressable("1");

            return endpointConfiguration;
        }
    }
}
