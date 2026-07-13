using UserManagementApi.Domain.Common;
using UserManagementApi.Domain.Entities;

namespace UserManagementApi.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<PagedResult<User>> GetPagedAsync(PagedRequest request);
    Task<int> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task InactivateAsync(int id);
}
