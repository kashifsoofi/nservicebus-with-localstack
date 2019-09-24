namespace Api.Core
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NServiceBus;
    using System;
    using System.Threading;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddHostedService<NServiceBusService>();
            services.AddSingleton<IHostedService, NServiceBusService>();
            services.AddSingleton(provider =>
            {
                var nServiceBusService = provider.GetService<IHostedService>() as NServiceBusService;
                if (nServiceBusService.MessageSession != null)
                {
                    return nServiceBusService.MessageSession;
                }

                var timeout = TimeSpan.FromSeconds(30);
                // SpinWait is here to accomodate for WebHost vs GenericHost difference
                // Closure here should be fine under the assumption we always fast track above once initialized
                if (!SpinWait.SpinUntil(() => nServiceBusService.MessageSession != null || nServiceBusService.StartupException != null,
                    timeout))
                {
                    throw new TimeoutException($"Unable to resolve the message session within '{timeout.ToString()}'. If you are trying to resolve the session within hosted services it is encouraged to use `Lazy<IMessageSession>` instead of `IMessageSession` directly");
                }

                nServiceBusService.StartupException?.Throw();

                return nServiceBusService.MessageSession;
            });
            services.AddSingleton(provider => new Lazy<IMessageSession>(provider.GetService<IMessageSession>));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
