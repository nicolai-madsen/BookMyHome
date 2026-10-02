using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MsSql;

namespace BookMyHome.Api.IntegrationTests
{
    public class BookMyHomeApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString());
        }

        public async ValueTask InitializeAsync() => await _dbContainer.StartAsync();

        public override async ValueTask DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
