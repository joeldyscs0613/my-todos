using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using MyTodos.Services.TodoService.Application.Shared.Contracts;

namespace MyTodos.Services.TodoService.Infrastructure.Persistence.Shared;

/// <summary>
/// Repository for fetching user options from Identity database
/// Uses Dapper for cross-database querying
/// </summary>
public sealed class AssigneeOptionsRepository : IAssigneeOptionsRepository
{
    private readonly string _identityDbConnectionString;

    public AssigneeOptionsRepository(IConfiguration configuration)
    {
        _identityDbConnectionString = configuration.GetConnectionString("IdentityServiceDb")
            ?? throw new InvalidOperationException("IdentityServiceDb connection string not configured");
    }

    public async Task<IReadOnlyList<UserOption>> GetUsersByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT
                Id,
                FirstName || ' ' || LastName AS FullName
            FROM Users
            WHERE TenantId = @TenantId
              AND IsActive = 1
              AND IsDeleted = 0
            ORDER BY FirstName, LastName";

        using var connection = new SqliteConnection(_identityDbConnectionString);
        await connection.OpenAsync(ct);

        var users = await connection.QueryAsync<UserOption>(
            new CommandDefinition(
                sql,
                new { TenantId = tenantId.ToString() },
                cancellationToken: ct));

        return users.ToList();
    }
}
