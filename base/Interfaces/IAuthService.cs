using Microsoft.AspNetCore.Mvc;

namespace baseApp.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(string email, string password, string fullName);
        Task<(string Token, string RefreshToken)> LoginAsync(string email, string password);
        Task<(string Token, string RefreshToken)> RefreshTokenAsync(string refreshToken);
        Task<IActionResult> LogoutAsync(string refreshToken);
    }
}
