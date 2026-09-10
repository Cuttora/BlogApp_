using BlogApp.Data;
using BlogApp.Models;
using BlogApp.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.UserName)
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        // Создаёт нового пользователя и автоматически присваивает ему роль
        // "User" (пункт 26 ТЗ). Вся работа с UserManager/RoleManager
        // инкапсулирована здесь - контроллер лишь передаёт введённые данные.
        public async Task<IdentityResultWrapper> CreateUserAsync(string userName, string email, string password)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                RegisteredAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return IdentityResultWrapper.Fail(result.Errors.Select(e => e.Description));
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            await _userManager.AddToRoleAsync(user, "User");
            return IdentityResultWrapper.Success(user.Id);
        }

        public async Task<IdentityResultWrapper> UpdateUserAsync(string id, string userName, string email, string? avatarUrl)
        {
            var existing = await _userManager.FindByIdAsync(id);
            if (existing == null)
            {
                return IdentityResultWrapper.Fail(new[] { "Пользователь не найден" });
            }

            // SetUserNameAsync/SetEmailAsync поддерживают в актуальном состоянии
            // NormalizedUserName/NormalizedEmail, обычное присваивание полей этого не делает.
            var userNameResult = await _userManager.SetUserNameAsync(existing, userName);
            if (!userNameResult.Succeeded)
            {
                return IdentityResultWrapper.Fail(userNameResult.Errors.Select(e => e.Description));
            }

            var emailResult = await _userManager.SetEmailAsync(existing, email);
            if (!emailResult.Succeeded)
            {
                return IdentityResultWrapper.Fail(emailResult.Errors.Select(e => e.Description));
            }

            existing.AvatarUrl = avatarUrl;

            var result = await _userManager.UpdateAsync(existing);
            return result.Succeeded
                ? IdentityResultWrapper.Success(existing.Id)
                : IdentityResultWrapper.Fail(result.Errors.Select(e => e.Description));
        }

        public async Task<IdentityResultWrapper> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return IdentityResultWrapper.Fail(new[] { "Пользователь не найден" });
            }

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded
                ? IdentityResultWrapper.Success()
                : IdentityResultWrapper.Fail(result.Errors.Select(e => e.Description));
        }
    }
}
