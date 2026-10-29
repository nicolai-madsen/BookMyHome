using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Testcontainers.MsSql;

namespace BookMyHome.Api.IntegrationTests
{
    public class BookMyHomeApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        // Image i konstruktøren i stedet for WithImage for at få warning til at fise af
        private readonly MsSqlContainer _dbContainer =
            new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString());

            builder.ConfigureTestServices(services =>
            {
                // Fake login: bruger header i stedet for rigtig JWT
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

                // Freeze the clock
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(
                    new FakeTimeProvider(TestsData.Today.ToDateTime(new TimeOnly(12, 0))));
            });
        }

        public async ValueTask InitializeAsync() => await _dbContainer.StartAsync();

        public override async ValueTask DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
            await base.DisposeAsync();
        }

        public HttpClient CreateClientAs(Guid userId)
        {
            var clients = CreateClient();
            clients.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
            return clients;
        }
    }
}