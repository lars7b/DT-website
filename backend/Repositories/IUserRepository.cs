using Backend.Models;

namespace Backend.Repositories;

public interface IUserRepository
{
    Task<User?> CreateUserWithCustomerAsync(User user, string firstName, string lastName);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, string newPasswordHash);
    Task<bool> ChangeEmailAsync(int userId, string newEmail);
}
