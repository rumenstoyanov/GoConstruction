//using Microsoft.AspNetCore.Mvc.Testing;
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
using Microsoft.Extensions.Configuration;
using GoApi.Data.Models;
using Newtonsoft.Json.Serialization;

namespace GoApi.Tests.Integration
{
    public class IntegrationTest: IDisposable
    {
        public IConfiguration Configuration { get; set; }
        protected readonly HttpClient TestClient;
        private readonly IServiceProvider _serviceProvider;
        private static ThrowawayDatabase _throwawayDatabase;
        protected readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() } };

        // Subclasses can access this method, private would mean only this class.
        protected IntegrationTest()
        {
            var builder = new ConfigurationBuilder().AddUserSecrets<IntegrationTest>();
            Configuration = builder.Build();
            var appFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Build an intermediate service provider so that we can resolve instances of services in this method.
                        //var intermediateProvider = services.BuildServiceProvider();
                        //using var intermediateScope = intermediateProvider.CreateScope();
                        //// Get the live database credentials.
                        //var pgSqlSettings = intermediateScope.ServiceProvider.GetService<PgSqlSettings>();

                        // Try this replacement method - still need to access the PgSqlSettings somehow though, maybe just parse those manually here?
                        //builder.UseSetting("","");

                        // Solution is, if we can access Configuration
                        // Remove all the singleton settings struct objects injected.
                        // Rebind configuration to them
                        // Mutate the IsEnabled
                        // Inject them as singletons again.
                        // They also left to be accessed in this body also e.g. in throwaway database.

                        // Remove certain configurations in order to mutate and reinject.
                        foreach (Type t in new Type[] { typeof(DbContextOptions<AppDbContext>), typeof(PgSqlSettings) })
                        {
                            var descriptor = services.SingleOrDefault(d => d.ServiceType == t);
                            if (descriptor != null)
                            {
                                services.Remove(descriptor);
                            }
                        }

                        // Reinject the PgSqlSettings (no mutation, we just need the object) for the throwaway database.
                        var pgSqlSettings = new PgSqlSettings();
                        Configuration.Bind("PgSqlSettings", pgSqlSettings);
                        services.AddSingleton(pgSqlSettings);

                        // Construct a throwaway database with it.
                        _throwawayDatabase = ThrowawayDatabase.Create(pgSqlSettings.Username, pgSqlSettings.Password, pgSqlSettings.Host);
                        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_throwawayDatabase.ConnectionString));


                    });
                });
            _serviceProvider = appFactory.Services;
            TestClient = appFactory.CreateClient();
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

        protected async Task<HttpResponseMessage> AttemptAuthenticationContractorAsync()
        {
            var content = new LoginRequestDto
            {
                Email = "r90876@gmail.com",
                Password = "usagafdgdzgdsgdsgsdgsdgdsf7"
            };
            var response = await TestClient.PostAsync("api/auth/login/", new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json"));
            return response;
        }

        public void Dispose()
        {
            _throwawayDatabase.Dispose();
        }
    }
}
