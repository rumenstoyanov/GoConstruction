﻿//using Microsoft.AspNetCore.Mvc.Testing;
//using System;
//using System.Collections.Generic;
//using System.Net.Http;
//using System.Text;
//using System.Linq;
//using GoApi.Data;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.EntityFrameworkCore.InMemory;
//using Microsoft.Extensions.DependencyInjection;
//using System.Threading.Tasks;
//using GoApi.Data.Dtos;

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GoApi.Data.Dtos;
using GoApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text;
using Newtonsoft.Json;
using ThrowawayDb.Postgres;
using GoApi.Data.Constants;

namespace GoApi.Tests.Integration
{
    public class IntegrationTest: IDisposable
    {
        protected readonly HttpClient TestClient;
        private readonly IServiceProvider _serviceProvider;
        private static ThrowawayDatabase _throwawayDatabase;

        // Subclasses can access this method, private would mean only this class.
        protected IntegrationTest()
        {
            var appFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Build an intermediate service provider so that we can resolve instances of services in this method.
                        var intermediateProvider = services.BuildServiceProvider();
                        // Get the live database credentials.
                        var pgSqlSettings = intermediateProvider.GetService<PgSqlSettings>();

                        // Remove the current db context - this is linked to the live database.
                        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Construct a throwaway database with it.
                        _throwawayDatabase = ThrowawayDatabase.Create(pgSqlSettings.Username, pgSqlSettings.Password, pgSqlSettings.Host);
                        //_throwawayDatabase = ThrowawayDatabase.Create("postgres", "learncode", "localhost");
                        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_throwawayDatabase.ConnectionString));


                    });
                });
            _serviceProvider = appFactory.Services;
            TestClient = appFactory.CreateClient();
            //using (var serviceScope = _serviceProvider.CreateScope())
            //{
            //    await Seed.SeedAsync(serviceScope);
            //}
            //var appDbContext = serviceScope.ServiceProvider.GetService<AppDbContext>();
            //appDbContext.Database.OpenConnection();
            //appDbContext.Database.EnsureCreated();
        }

        protected async Task<HttpResponseMessage> RegisterContractorAsync()
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                await Seed.SeedAsync(serviceScope);
            }

            var content = new RegisterContractorRequestDto
            {
                Email = "r90876@gmail.com",
                FullName = "Matthew Test",
                Password = "usagafdgdzgdsgdsgsdgsdgdsf7",
                ConfirmPassword = "usagafdgdzgdsgdsgsdgsdgdsf7",
                OrganisationName = "Test Corp X",
                PhoneNumber = "07450022666",
                Address = "TES123",
                Industry = "Testing Work"

            };
            var response = await TestClient.PostAsync("api/auth/register/contractor", new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json"));
            return response;
        }

        public void Dispose()
        {
            _throwawayDatabase.Dispose();
            //using var serviceScope = _serviceProvider.CreateScope();
            //var appDbContext = serviceScope.ServiceProvider.GetService<AppDbContext>();
        }
    }
}
