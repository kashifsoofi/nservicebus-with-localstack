using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Api.Core
{
    using System.Threading;

    public class Program
    {
        public static void Main(string[] args)
        {
            Thread.Sleep(15000);
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
