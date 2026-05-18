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

        var createdCustomer = await _userRepository.CreateUserWithCustomerAsync(newUser);
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
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
