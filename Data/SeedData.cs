using BlogApp.Models;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Data
{
    // Наполняет базу ролями Admin/Moderator/User и тремя демонстрационными
    // пользователями при первом запуске приложения.
    public static class SeedData
    {
        private static readonly string[] Roles = { "Admin", "Moderator", "User" };

        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            await CreateUserIfMissingAsync(userManager, "admin@blog.com", "Password123!", "Admin");
            await CreateUserIfMissingAsync(userManager, "moderator@blog.com", "Password123!", "Moderator");
            await CreateUserIfMissingAsync(userManager, "user@blog.com", "Password123!", "User");
        }

        private static async Task CreateUserIfMissingAsync(
            UserManager<ApplicationUser> userManager, string email, string password, string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                RegisteredAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
