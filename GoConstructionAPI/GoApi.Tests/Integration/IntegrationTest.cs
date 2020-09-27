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
using GoApi.Installers;
using Xunit.Abstractions;
using StackExchange.Redis;
using GoApi.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace GoApi.Tests.Integration
{
    public class IntegrationTest: IDisposable
    {
        private readonly ITestOutputHelper _output;
        public IConfiguration Configuration { get; set; }
        protected readonly HttpClient TestClient;
        private readonly IServiceProvider _serviceProvider;
        private static ThrowawayDatabase _throwawayDatabase;
        protected readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() } };

        // Subclasses can access this method, private would mean only this class.
        protected IntegrationTest(ITestOutputHelper output)
        {
            _output = output;
            // The configuration we use for integration testing has Caching and Mailing disabled (flags set to false) - we do not test these aspects here for now.  
            // These mutations tested to work in .NET Core 3.1
            var builder = new ConfigurationBuilder().AddUserSecrets<IntegrationTest>();
            Configuration = builder.Build();

            var appFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {

                        // Remove certain injected services (from the Startup.cs) in order to mutate and reinject.
                        foreach (Type t in new Type[] { 
                            typeof(DbContextOptions<AppDbContext>), 
                            typeof(PgSqlSettings),
                            typeof(RedisSettings),
                            typeof(MailSettings),
                            typeof(JwtSettings),
                            typeof(ConnectionMultiplexer),
                            typeof(RedisCacheService)
                        })
                        {
                            var descriptor = services.SingleOrDefault(d => d.ServiceType == t);
                            if (descriptor != null)
                            {
                                services.Remove(descriptor);
                            }
                        }

                        // Reinject the settings with the new configuration (these mutations tested to work in .NET Core 3.1)
                        var settings = ConfigurationInstaller.BindSettings(Configuration, services, disableAll: true);
                        ConfigurationInstaller.InstallServicesFromSettings(settings, services);
                        _output.WriteLine(JsonConvert.SerializeObject(settings));
                        // Construct a throwaway database with it.
                        _throwawayDatabase = ThrowawayDatabase.Create(settings.PgSqlSettings.Username, settings.PgSqlSettings.Password, settings.PgSqlSettings.Host);
                        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_throwawayDatabase.ConnectionString));


                    });
                });
            _serviceProvider = appFactory.Services;
            TestClient = appFactory.CreateClient();
        }

        protected async Task<HttpResponseMessage> RegisterContractorOneAsync()
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                await Seed.SeedAsync(serviceScope);
            }

            var content = new RegisterContractorRequestDto
            {
                Email = TestConstants.ContractorOneEmail,
                FullName = "Matthew Test",
                Password = TestConstants.ContractorOnePassword,
                ConfirmPassword = TestConstants.ContractorOnePassword,
                OrganisationName = "Test Corp X",
                PhoneNumber = "000000000000000",
                Address = "TES123",
                Industry = "Testing Work"

            };
            var response = await TestClient.PostAsync("api/auth/register/contractor", new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json"));

            return response;
        }

        protected async Task<HttpResponseMessage> AttemptAuthenticationContractorOneAsync()
        {
            var content = new LoginRequestDto
            {
                Email = TestConstants.ContractorOneEmail,
                Password = TestConstants.ContractorOnePassword
            };
            var response = await TestClient.PostAsync("api/auth/login/", new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json"));
            return response;
        }

        protected async Task ConfirmContractorOneEmailAsync()
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByEmailAsync(TestConstants.ContractorOneEmail);
                var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                await userManager.ConfirmEmailAsync(user, emailToken);
            }
        }

        protected async Task LoginContractorOne()
        {
            await RegisterContractorOneAsync();
            await ConfirmContractorOneEmailAsync();
            var response = JsonConvert.DeserializeObject<LoginResponseDto>(await (await AttemptAuthenticationContractorOneAsync()).Content.ReadAsStringAsync());
            TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, response.AccessToken);
        }

        public void Dispose()
        {
            _throwawayDatabase.Dispose();
        }
    }
}
