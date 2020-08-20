using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GoApi.Data;
using GoApi.Data.Constants;
using GoApi.Data.Models;
using GoApi.Services.Implementations;
using GoApi.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using static GoApi.Data.Constants.Seniority;

namespace GoApi
{
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
            var jwtSettings = new JwtSettings();
            Configuration.Bind("JwtSettings", jwtSettings);
            var mailSettings = new MailSettings();
            Configuration.Bind("MailSettings", mailSettings);

            services.AddControllers();
            services.AddSingleton(jwtSettings);
            services.AddSingleton(mailSettings);
            services.AddDbContext<UserDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PgDbMain")));
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PgDbMain")));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<UserDbContext>()
                    .AddDefaultTokenProviders();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMailService, MailService>();
            services.Configure<IdentityOptions>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                options.SignIn.RequireConfirmedEmail = true;
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["JwtSettings:Issuer"],
                        ValidAudience = Configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSettings:SigningKey"]))
                    };
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AdminOnlyPolicy, policy => policy.RequireClaim(SeniorityClaimKey, Admin));
                options.AddPolicy(ContractorOrAbovePolicy, policy => policy.RequireClaim(SeniorityClaimKey, Admin, Contractor));
                options.AddPolicy(ManagerOrAbovePolicy, policy => policy.RequireClaim(SeniorityClaimKey, Admin, Contractor, Manager));
                options.AddPolicy(SupervisorOrAbovePolicy, policy => policy.RequireClaim(SeniorityClaimKey, Admin, Contractor, Manager, Supervisor));
                options.AddPolicy(WorkerOrAbovePolicy, policy => policy.RequireClaim(SeniorityClaimKey, Admin, Contractor, Manager, Supervisor, Worker));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
