using Xunit;
using Moq;
using System.Threading.Tasks;
using baseApp.Services;
using baseApp.Data;
using baseApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace baseApp.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Jwt:Key", "super_secret_key_for_testing_test_login_jwt" }
            })
            .Build();

            _context = new AppDbContext(options);
            _authService = new AuthService(_context, new TokenService(configuration));

            SeedAsync(_context).Wait();
        }

        private static async Task SeedAsync(AppDbContext context)
        {
            if (!await context.Roles.AnyAsync())
            {
                context.Roles.Add(new Role
                {
                    Name = "User"
                });

                await context.SaveChangesAsync();
            }
        }

        [Fact]
        [Trait("Category", "Register")]
        public async Task RegisterAsync_Should_Create_User()
        {
            var userId = await _authService.RegisterAsync("test@example.com", "Password123!", "Test User");

            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            Assert.NotNull(user);
            Assert.Equal("test@example.com", user.Email);
        }

        [Fact]
        [Trait("Category", "Login")]
        public async Task LoginAsync_ValidCredentials_ShouldReturnTokens()
        {
            string email = "test@example.com";
            string password = "Password123";
            string fullName = "Test User";

            await _authService.RegisterAsync(email, password, fullName);

            var (token, refreshToken) = await _authService.LoginAsync(email, password);

            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.False(string.IsNullOrWhiteSpace(refreshToken));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
