using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoLibrary.Data.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GoApp.Console
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var serviceBusSettings = new ServiceBusSettings();
            Configuration.Bind("ServiceBus", serviceBusSettings);
            var mailSettings = new MailSettings();
            Configuration.Bind("MailSettings", mailSettings);


            services.AddSingleton(serviceBusSettings);
            services.AddSingleton(mailSettings);
            services.AddSingleton<IQueueClient>(_ => new QueueClient(serviceBusSettings.ConnectionString, serviceBusSettings.QueueName));
            services.AddHostedService<ServiceBusConsumer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
        }
    }
}
