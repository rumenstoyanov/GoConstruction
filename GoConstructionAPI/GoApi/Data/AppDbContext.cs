using GoApi.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<Organisation> Organisations { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<Update> Updates { get; set; }
        public DbSet<JobStatus> JobStatuses { get; set; }
        public DbSet<Job> Jobs { get; set; }
    }
}
