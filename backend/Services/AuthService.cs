using Backend.DTOs;
using Backend.Repositories;
using Backend.Models;
using Isopoh.Cryptography.Argon2;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto request)
    {
        var hashedPassword = Argon2.Hash(request.Password);

        var newUser = new User
        {
            Email = request.Email,
            PasswordHash = hashedPassword,
            Role = "Customer"
        };

        var createdCustomer = await _userRepository.CreateUserWithCustomerAsync(newUser, request.FirstName, request.LastName);
        if (createdCustomer == null)
        {
            return (false, "Registration failed.");
        }
        return (true, "Registration successful.");
    }

    public async Task<string?> LoginAsync(LoginDto request)
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email);

        if (user == null) return null;

        if (!Argon2.Verify(user.PasswordHash, request.Password)) return null;

        return GenerateJwtToken(user);
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordDto passwordDetails)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId);
        if(user == null) return (false, "Update failed.");
        bool isValid = Argon2.Verify(user.PasswordHash, passwordDetails.CurrentPassword);
        if (!isValid) return (false, "Update failed.");

        string newPasswordHash = Argon2.Hash(passwordDetails.NewPassword);
        var updateStatus = await _userRepository.ChangePasswordAsync(userId, newPasswordHash);
       
        if (!updateStatus) return (false, "Update failed.");

        return (true, "Password changed successfully.");
    }

    public async Task<(bool Success, string Message)> ChangeEmailAsync(int userId, ChangeEmailDto emailDetails)
    {
        var updateStatus = await _userRepository.ChangeEmailAsync(userId, emailDetails.NewEmail);
       
        if (!updateStatus) return (false, "Update failed.");

        return (true, "Email changed successfully.");
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
