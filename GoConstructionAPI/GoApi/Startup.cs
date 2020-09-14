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
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;
using static GoApi.Data.Constants.Seniority;
using Swashbuckle.AspNetCore.Filters;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using System.Threading;

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
            var redisSettings = new RedisSettings();
            Configuration.Bind("RedisSettings", redisSettings);

            services.AddControllers().AddNewtonsoftJson(s => {
                s.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            services.AddSingleton(jwtSettings);
            services.AddSingleton(mailSettings);
            services.AddSingleton(redisSettings);
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PgDbMain")));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMailService, MailService>();
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddScoped<IUpdateService, UpdateService>();
            services.AddScoped<IResourceService, ResourceService>();
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisSettings.ConnectionString));
            services.AddSingleton<ICacheService, RedisCacheService>();
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
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey))
                    };
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
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference
                            {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            ThreadPool.SetMinThreads(4000, 100); // Set the Redis worker and IOCP threads minimums.
        }
    }
}
