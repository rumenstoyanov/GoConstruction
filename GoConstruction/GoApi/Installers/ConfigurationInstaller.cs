using GoApi.Data.Constants;
using GoApi.Services.Implementations;
using GoApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GoApi.Profiles;
using Microsoft.Azure.ServiceBus;
using GoLibrary.Data.Constants;

namespace GoApi.Installers
{
    public static class ConfigurationInstaller
    {
        public static Settings BindSettings(IConfiguration configuration, IServiceCollection services, bool disableAll = false)
        {
            var jwtSettings = new JwtSettings();
            configuration.Bind("JwtSettings", jwtSettings);
            var mailSettings = new MailSettings();
            configuration.Bind("MailSettings", mailSettings);
            var redisSettings = new RedisSettings();
            configuration.Bind("RedisSettings", redisSettings);
            var pgSqlSettings = new PgSqlSettings();
            configuration.Bind("PgSqlSettings", pgSqlSettings);
            var serviceBusSettings = new ServiceBusSettings();
            configuration.Bind("ServiceBus", serviceBusSettings);

            if (disableAll)
            {
                mailSettings.IsEnabled = false;
                redisSettings.IsEnabled = false;
            }


            services.AddSingleton(jwtSettings);
            services.AddSingleton(mailSettings);
            services.AddSingleton(redisSettings);
            services.AddSingleton(pgSqlSettings);
            services.AddSingleton(serviceBusSettings);

            return new Settings
            {
                JwtSettings = jwtSettings,
                PgSqlSettings = pgSqlSettings,
                RedisSettings = redisSettings,
                MailSettings = mailSettings,
                ServiceBusSettings = serviceBusSettings
            };

        }

        public static MapperConfiguration AssembleMapperConfiguration()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.AddProfile(new ApplicationUserProfile());
                cfg.AddProfile(new CommentProfile());
                cfg.AddProfile(new JobProfile());
                cfg.AddProfile(new JobStatusProfile());
                cfg.AddProfile(new OrganisationProfile());
                cfg.AddProfile(new SiteProfile());
                cfg.AddProfile(new UpdateProfile());
            });

            return config;
        }

        public static void InstallServicesFromSettings(Settings settings, IServiceCollection services)
        {
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(settings.RedisSettings.ConnectionString));
            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddSingleton<IQueueClient>(_ => new QueueClient(settings.ServiceBusSettings.ConnectionString, settings.ServiceBusSettings.QueueName));
            services.AddSingleton<IMessagePublisher, MessagePublisher>();
        }
    }
}
