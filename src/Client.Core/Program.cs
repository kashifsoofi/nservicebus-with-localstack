using System;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using NServiceBus;
using NServiceBus.Logging;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.FullDuplex.Client";
        LogManager.Use<DefaultFactory>()
            .Level(LogLevel.Info);
        var endpointConfiguration = new EndpointConfiguration("Samples.FullDuplex.Client");
        endpointConfiguration.DoNotCreateQueues();

        var transport = endpointConfiguration.UseTransport<SqsTransport>();
        transport.ClientFactory(() => new AmazonSQSClient(
            new AnonymousAWSCredentials(),
            new AmazonSQSConfig
            {
                ServiceURL = "http://localhost:4576"
            }));

        var s3Configuration = transport.S3("bucketname", "my/key/prefix");
        s3Configuration.ClientFactory(() => new AmazonS3Client(
            new AnonymousAWSCredentials(),
            new AmazonS3Config
            {
                ServiceURL = "http://localhost:4572"
            }));

        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.UsePersistence<InMemoryPersistence>();

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        Console.WriteLine("Press enter to send a message");
        Console.WriteLine("Press any key to exit");

        #region ClientLoop

        while (true)
        {
            var key = Console.ReadKey();
            Console.WriteLine();

            if (key.Key != ConsoleKey.Enter)
            {
                break;
            }
            var guid = Guid.NewGuid();
            Console.WriteLine($"Requesting to get data by id: {guid:N}");

            var message = new RequestDataMessage
            {
                DataId = guid,
                String = "String property value"
            };
            await endpointInstance.Send("Samples.FullDuplex.Server", message)
                .ConfigureAwait(false);
        }

        #endregion
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}