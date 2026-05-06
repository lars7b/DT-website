using Backend.DTOs;

namespace Backend.Services;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterDto request);
    Task<string?> LoginAsync(LoginDto request);
    Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordDto request);
    Task<(bool Success, string Message)> ChangeEmailAsync(int userId, ChangeEmailDto request);
}