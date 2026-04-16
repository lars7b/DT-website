using Backend.Models;

namespace Backend.Repositories;

public interface IUserRepository
{
    Task<User?> CreateUserWithCustomerAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
}
