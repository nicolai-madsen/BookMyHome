using BookMyHome.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

public class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public async ValueTask InitializeAsync() => await _container.StartAsync();
    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public BookMyHomeContext CreateContext(string dbName)
    {
        var cs = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = dbName
        }.ConnectionString;

        var options = new DbContextOptionsBuilder<BookMyHomeContext>()
            .UseSqlServer(cs)
            .Options;

        var context = new BookMyHomeContext(options);
        context.Database.Migrate();   // opretter db'en første gang, no-op derefter
        return context;
    }
}

[CollectionDefinition("SqlServer")]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture>;