namespace Server.Core
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System.Threading.Tasks;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.SimpleNotificationService;
    using Amazon.SQS;
    using Shared.Core.Config;
    using Microsoft.Extensions.Configuration;
    using NServiceBus;
    using Serilog;
    using Serilog.Events;

    class Program
    {
        static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            IConfiguration configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", true, true)
              .AddEnvironmentVariables()
              .Build();

            var host = new HostBuilder()
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<NServiceBusOptions>(configuration.GetSection("NServiceBus"));
                })
                .UseSerilog()
                .UseNServiceBus(context =>
                {
                    var nServiceBusOptions = configuration.GetSection("NServiceBus").Get<NServiceBusOptions>();

                    var endpointConfiguration = new EndpointConfiguration("Samples.FullDuplex.Server");
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

                    return endpointConfiguration;
                })
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();
        }
    }
}