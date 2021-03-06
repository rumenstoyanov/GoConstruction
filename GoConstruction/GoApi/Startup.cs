using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GoApi.Data.Constants;
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
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;
using static GoApi.Data.Constants.Seniority;
using Swashbuckle.AspNetCore.Filters;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using System.Threading;
using GoApi.Installers;
using System.Net;
using GoLibrary.Data.Models;
using GoLibrary.Data;
using GoApi.Data;
using Microsoft.Azure.ServiceBus;

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

            services.AddControllers().AddNewtonsoftJson(s => {
                s.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            var settings = ConfigurationInstaller.BindSettings(Configuration, services);
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PgDbMain")));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();
            services.AddSingleton(ConfigurationInstaller.AssembleMapperConfiguration().CreateMapper());
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMailService, MailService>();
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddScoped<IUpdateService, UpdateService>();
            services.AddScoped<IResourceService, ResourceService>();
            ConfigurationInstaller.InstallServicesFromSettings(settings, services);
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

            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = settings.JwtSettings.Issuer,
                ValidAudience = settings.JwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtSettings.SigningKey))
            };
            services.AddSingleton(tokenValidationParameters);
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
                    options.TokenValidationParameters = tokenValidationParameters;
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AdminOnlyPolicy, policy => 
                { 
                    policy.RequireClaim(SeniorityClaimKey, Admin);
                    policy.RequireClaim(IsInitalSetClaimKey, true.ToString());
                
                });
                options.AddPolicy(ContractorOrAbovePolicy, policy => 
                { 
                    policy.RequireClaim(SeniorityClaimKey, Admin, Contractor);
                    policy.RequireClaim(IsInitalSetClaimKey, true.ToString());

                });
                options.AddPolicy(ManagerOrAbovePolicy, policy => 
                { 
                    policy.RequireClaim(SeniorityClaimKey, Admin, Contractor, Manager);
                    policy.RequireClaim(IsInitalSetClaimKey, true.ToString());
                });
                options.AddPolicy(SupervisorOrAbovePolicy, policy => 
                { 
                    policy.RequireClaim(SeniorityClaimKey, Admin, Contractor, Manager, Supervisor);
                    policy.RequireClaim(IsInitalSetClaimKey, true.ToString());

                });
                options.AddPolicy(WorkerOrAbovePolicy, policy => 
                { 
                    policy.RequireClaim(SeniorityClaimKey, Admin, Contractor, Manager, Supervisor, Worker);
                    policy.RequireClaim(IsInitalSetClaimKey, true.ToString());
                });
            });
            services.AddSwaggerGen(c => 
            {
                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Description = "",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference
                            {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                            },
                            Scheme = "oauth2",
                            Name = JwtBearerDefaults.AuthenticationScheme,
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseAuthentication();
            app.UseHttpsRedirection();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MVP API");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseStatusCodePages(async context =>
            {
                var response = context.HttpContext.Response;

                if (response.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    response.Redirect("/swagger/");
                }

            });

            ThreadPool.SetMinThreads(4000, 100); // Set the Redis worker and IOCP threads minimums.
        }
    }
}
