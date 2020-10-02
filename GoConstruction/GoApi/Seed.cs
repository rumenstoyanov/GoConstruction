using GoApi.Data;
using GoApi.Data.Constants;
using GoLibrary.Data.Models;
using GoLibrary.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GoApi.Data.Constants.Seniority;

namespace GoApi
{
    public static class Seed
    {
        public static async Task SeedAsync(IServiceScope serviceScope)
        {
            var appDbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Migrate
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

            // Seed job statuses
            if (!await appDbContext.JobStatuses.AnyAsync())
            {
                foreach (string status in JobStatuses.StatusList)
                {
                    appDbContext.Add(new JobStatus { Title = status });
                }
                await appDbContext.SaveChangesAsync();
            }
        }
    }
}
