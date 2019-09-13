using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Core;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var host = new HostBuilder()
            .ConfigureServices((hostBuilderContext, services) => { services.AddHostedService<NServiceBusService>(); })
            .UseConsoleLifetime()
            .Build();

        await host.RunAsync();
    }
}