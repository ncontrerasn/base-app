namespace baseApp.Controllers;

using Microsoft.AspNetCore.Mvc;
using baseApp.Interfaces;
using baseApp.Dtos;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var userId = await _authService.RegisterAsync(dto.Email, dto.Password, dto.FullName);
        return Ok(new { userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (token, refresh) = await _authService.LoginAsync(dto.Email, dto.Password);
        return Ok(new { token, refresh });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var (token, refresh) = await _authService.RefreshTokenAsync(refreshToken);
        return Ok(new { token, refresh });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        await _authService.LogoutAsync(refreshToken);
        return Ok(new { message = "User logged out successfully" });
    }

}

