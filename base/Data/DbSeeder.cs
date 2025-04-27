using baseApp.Helpers;
using baseApp.Models;
using Microsoft.EntityFrameworkCore;

namespace baseApp.Data;
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
                {
                    new() { Name = "Admin" },
                    new() { Name = "User" }
                };

            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var admin = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                FullName = "Admin User",
                PasswordHash = HashHelper.HashPassword("admin"),
                UserRoles = new List<UserRole>()
            };

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

            admin.UserRoles.Add(new UserRole
            {
                RoleId = adminRole.Id,
                UserId = admin.Id
            });

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
