using Dapper;
using UserManagementApi.Domain.Common;
using UserManagementApi.Domain.Entities;
using UserManagementApi.Domain.Repositories;

namespace UserManagementApi.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ConnectionFactory _connectionFactory;

    public UserRepository(ConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Nome, Email, SenhaHash, Role, Ativo
            FROM Users
            WHERE Id = @Id AND Ativo = 1";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT Id, Nome, Email, SenhaHash, Role, Ativo
            FROM Users
            WHERE Email = @Email";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<PagedResult<User>> GetPagedAsync(PagedRequest request)
    {
        const string sql = @"
            SELECT Id, Nome, Email, SenhaHash, Role, Ativo
            FROM Users
            WHERE Ativo = 1
            ORDER BY Nome
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Users
            WHERE Ativo = 1;";

        using var connection = _connectionFactory.CreateConnection();
        var offset = (request.Page - 1) * request.PageSize;

        using var multi = await connection.QueryMultipleAsync(sql, new { Offset = offset, request.PageSize });
        var items = (await multi.ReadAsync<User>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<int> CreateAsync(User user)
    {
        const string sql = @"
            INSERT INTO Users (Nome, Email, SenhaHash, Role, Ativo)
            OUTPUT INSERTED.Id
            VALUES (@Nome, @Email, @SenhaHash, @Role, @Ativo);";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task UpdateAsync(User user)
    {
        const string sql = @"
            UPDATE Users
            SET Nome = @Nome, Email = @Email, SenhaHash = @SenhaHash, Role = @Role
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, user);
    }

    public async Task InactivateAsync(int id)
    {
        const string sql = "UPDATE Users SET Ativo = 0 WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
