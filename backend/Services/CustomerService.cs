using Backend.Models;
using Backend.DTOs;
using Backend.Repositories;
using Isopoh.Cryptography.Argon2;

namespace Backend.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUserRepository _userRepository;

    public CustomerService(ICustomerRepository customerRepository, IUserRepository userRepository)
    {
        _customerRepository = customerRepository;
        _userRepository = userRepository;
    }

    public async Task<CustomerDto?> GetCustomerAsync(int userId)
    {
        var customer = await _customerRepository.GetCustomerAsync(userId);

        if (customer == null) return null;

        return new CustomerDto
        {
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Phone = customer.Phone,
            Address = customer.Address
        };
    }

    public async Task<(bool Success, string Message)> UpdateCustomerAsync(int userId, CustomerDto customerDetails)
    {
        var updateStatus = await _customerRepository.UpdateCustomerAsync(userId, customerDetails);

        if (!updateStatus) return (false, "Update failed.");

        return (true, "Customer details updated.");
    }

    public async Task<(bool Success, string Message)> DeleteCustomerAsync(int userId, string password)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId);
        if(user == null) return (false, "Update failed.");
        bool isValid = true;//Argon2.Verify(user.PasswordHash, password);
        if (!isValid) return (false, "Update failed.");

        var deleteStatus = await _customerRepository.DeleteCustomerAsync(userId);
        
        if (!deleteStatus) return (false, "Deletion failed.");
        
        return (true, "Customer deleted.");
    }
}