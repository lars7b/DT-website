using Backend.DTOs;

namespace Backend.Services;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterDto request);
    Task<string?> LoginAsync(LoginDto request);
}