using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Core;
using System.Threading.Tasks;
using Shared.Core.Config;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main()
    {
        Thread.Sleep(15000);

        IConfiguration configuration = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json", true, true)
          .AddEnvironmentVariables()
          .Build();

        var host = new HostBuilder()
            .ConfigureServices((hostBuilderContext, services) =>
            {
                services.AddOptions();
                services.Configure<NServiceBusOptions>(configuration.GetSection("NServiceBus"));
                services.AddHostedService<NServiceBusService>();
            })
            .UseConsoleLifetime()
            .Build();

        await host.RunAsync();
    }
}