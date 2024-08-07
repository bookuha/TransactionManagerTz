using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TransactionManager.Data.Database;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateConnectionAsync();
}

public class NpgsqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    public async Task<DbConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(configuration["PostgresConnectionString"]);
        await connection.OpenAsync();
        return connection;
    }
}