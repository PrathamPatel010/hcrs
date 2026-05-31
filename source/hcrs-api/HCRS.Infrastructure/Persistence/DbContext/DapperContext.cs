using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HCRS.Infrastructure.Persistence.DbContext;

public class DapperContext
{
    private readonly string _connectionString;
    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param=null)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<T>(sql, param);
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, object? param=null)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<T>(sql, param);
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteAsync(sql, param);
    }
}
