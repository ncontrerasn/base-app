using Microsoft.AspNetCore.Identity;

namespace baseApp.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<RefreshToken> RefreshTokens { get; set; } = new();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
