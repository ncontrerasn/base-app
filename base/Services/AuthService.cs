namespace baseApp.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using baseApp.Data;
using baseApp.Interfaces;
using baseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(AppDbContext context, ITokenService tokenService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _tokenService = tokenService;
        _hasher = new PasswordHasher<User>();
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> RegisterAsync(string email, string password, string fullName)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email))
            throw new Exception("User already exists");

        // Buscamos el rol User
        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
            throw new Exception("Default role 'User' not found. Please seed roles first.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = fullName,
            PasswordHash = _hasher.HashPassword(null, password),
            UserRoles = new List<UserRole> // <-- Aquí hacemos la relación
        {
            new UserRole
            {
                RoleId = userRole.Id // El ID del rol "User"
            }
        }
        };

        user.PasswordHash = _hasher.HashPassword(user, password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user.Id.ToString();
    }

    public async Task<(string Token, string RefreshToken)> LoginAsync(string email, string password)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new Exception("Invalid credentials");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            throw new Exception("Invalid credentials");

        var jwt = _tokenService.GenerateToken(user);
        var refresh = _tokenService.GenerateRefreshToken();

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refresh,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            UserId = user.Id
        });

        await _context.SaveChangesAsync();

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,  
            Secure = false,    // true solo para HTTPS
            Expires = DateTime.UtcNow.AddDays(7)
        };

        _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refresh, cookieOptions);

        return (jwt, refresh);
    }

    public async Task<(string Token, string RefreshToken)> RefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens.Include(r => r.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

        if (token is null)
            throw new Exception("Invalid refresh token");

        token.IsRevoked = true;
        var newRefresh = _tokenService.GenerateRefreshToken();

        token.User!.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefresh,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            UserId = token.User.Id
        });

        await _context.SaveChangesAsync();
        var jwt = _tokenService.GenerateToken(token.User);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,    // true solo para HTTPS
            Expires = DateTime.UtcNow.AddDays(7)
        };

        _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", newRefresh, cookieOptions);

        return (jwt, newRefresh);
    }

    public async Task<IActionResult> LogoutAsync(string refreshToken)
    {
        // Buscar el refresh token en la base de datos
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null)
        {
            return new BadRequestObjectResult("Invalid refresh token");
        }

        // Revocar el refresh token (marcarlo como invalidado)
        token.IsRevoked = true;

        await _context.SaveChangesAsync();

        _httpContextAccessor.HttpContext.Response.Cookies.Delete("refreshToken");

        return new NoContentResult(); // Devuelve 204 No Content si todo va bien
    }
}
