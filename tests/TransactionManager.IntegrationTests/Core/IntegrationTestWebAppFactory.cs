using System.Data.Common;
using Npgsql;
using Respawn;

namespace TransactionManager.IntegrationTests.Core;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Data;
using Data.Database;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:14-bullseye")
        .WithPassword("test_password")
        .Build();

    public HttpClient HttpClient { get; private set; } = default!;
    public IDbConnectionFactory DbConnectionFactory = default!;
    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("PostgresConnectionString", _dbContainer.GetConnectionString());

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TransactionManagerDbContext>();
            services.RemoveAll<IDbConnectionFactory>();

            services.AddDbContext<TransactionManagerDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));

            services.AddTransient<IDbConnectionFactory, NpgsqlConnectionFactory>();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        HttpClient = CreateClient();
        DbConnectionFactory = Services.GetRequiredService<IDbConnectionFactory>();
        await InitializeRespawner();
    }

    private async Task InitializeRespawner()
    {
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}