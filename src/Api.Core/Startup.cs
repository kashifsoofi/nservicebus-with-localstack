namespace Api.Core
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NServiceBus;
    using System;
    using System.Threading;
    using Microsoft.Extensions.Hosting;
    using Shared.Core.Config;
    using Microsoft.OpenApi.Models;

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
            services.AddOptions();
            services.Configure<NServiceBusOptions>(Configuration.GetSection("NServiceBus"));

            services.AddControllers();

            services.AddSingleton<NServiceBusService>();
            services.AddSingleton<IHostedService, NServiceBusService>(
                serviceProvider => serviceProvider.GetService<NServiceBusService>());
            services.AddSingleton(provider =>
            {
                var nServiceBusService = provider.GetService<NServiceBusService>();
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
                    throw new TimeoutException($"Unable to resolve the message session within '{timeout}'. If you are trying to resolve the session within hosted services it is encouraged to use `Lazy<IMessageSession>` instead of `IMessageSession` directly");
                }

                nServiceBusService.StartupException?.Throw();

                return nServiceBusService.MessageSession;
            });
            services.AddSingleton(provider => new Lazy<IMessageSession>(provider.GetService<IMessageSession>));

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FullDuplex Messages Api", Version = "v1" });
            });
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

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });

            // app.UseHttpsRedirection();
            app.UseRouting();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapControllers();
            });
        }
    }
}
