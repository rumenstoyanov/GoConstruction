using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoApi.Data;
using GoApi.Data.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static GoApi.Data.Constants.Seniority;

namespace GoApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                // Get contexts
                var userDbContext = serviceScope.ServiceProvider.GetRequiredService<UserDbContext>();
                var appDbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Migrate
                await userDbContext.Database.MigrateAsync();
                await appDbContext.Database.MigrateAsync();

                // Seed roles if not present
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                foreach (string role in new string[] { Admin, Contractor, Manager, Supervisor, Worker })
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var idRole = new IdentityRole(role);
                        await roleManager.CreateAsync(idRole);
                    }
                }
            }

            // Run
            await host.RunAsync();
        }

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureWebHostDefaults(webBuilder =>
        //        {
        //            webBuilder.UseStartup<Startup>();
        //        });
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
                    WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>();
    }
}
