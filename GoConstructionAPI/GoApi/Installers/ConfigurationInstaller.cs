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

            if (disableAll)
            {
                mailSettings.IsEnabled = false;
                redisSettings.IsEnabled = false;
            }


            services.AddSingleton(jwtSettings);
            services.AddSingleton(mailSettings);
            services.AddSingleton(redisSettings);
            services.AddSingleton(pgSqlSettings);

            return new Settings
            {
                JwtSettings = jwtSettings,
                PgSqlSettings = pgSqlSettings,
                RedisSettings = redisSettings,
                MailSettings = mailSettings
            };

        }

        public static void InstallServicesFromSettings(Settings settings, IServiceCollection services)
        {
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(settings.RedisSettings.ConnectionString));
            //services.AddSingleton<ICacheService>(new RedisCacheService(services.BuildServiceProvider().GetRequiredService<IConnectionMultiplexer>(), services.BuildServiceProvider().GetRequiredService<RedisSettings>()));
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
    }
}
