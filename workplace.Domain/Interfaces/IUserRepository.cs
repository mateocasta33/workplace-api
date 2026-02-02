using Azure;
using workplace.Domain.Entities;

namespace workplace.Domain.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User?>> GetAllUsersAsync();
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User?> UpdateUserAsync(User user, ETag eTag);
    Task<bool> DeleteUserAsync(User user);
    Task<User> CreateUserAsync(User user);
}