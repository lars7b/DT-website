using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Backend.DTOs;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    private int GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!.Value);
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
        {
            return Conflict(new { message = result.Message });
        }
        return Ok(new { message = result.Message });
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var token = await _authService.LoginAsync(request);

        if (token == null) return Unauthorized(new { message = "Ongeldig e-mailadres of wachtwoord" });

        return Ok(new { token });
    }

    [Authorize]
    [HttpPut("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        int userId = GetUserIdFromToken();

        var result = await _authService.ChangePasswordAsync(userId, request);
        if (!result.Success)
        {
            return Conflict(new { message = result.Message });
        }
        return Ok(new { message = result.Message });
    }

    [Authorize]
    [HttpPut("change-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto request)
    {
        int userId = GetUserIdFromToken();

        var result = await _authService.ChangeEmailAsync(userId, request);
        if (!result.Success)
        {
            return Conflict(new { message = result.Message });
        }
        return Ok(new { message = result.Message });
    }
}