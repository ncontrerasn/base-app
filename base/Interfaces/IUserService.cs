using baseApp.Models;

namespace baseApp.Interfaces;

public interface IUserService
{
    Task<User> GetUserByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task UpdateUserAsync(Guid id, string fullName);
    Task DeleteUserAsync(Guid id);
}

