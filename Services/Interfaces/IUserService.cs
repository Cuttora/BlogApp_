using BlogApp.Models;

namespace BlogApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser?> GetUserByIdAsync(string id);
        Task<IdentityResultWrapper> CreateUserAsync(string userName, string email, string password);
        Task<IdentityResultWrapper> UpdateUserAsync(string id, string userName, string email, string? avatarUrl);
        Task<IdentityResultWrapper> DeleteUserAsync(string id);
    }

    // Небольшая обёртка, чтобы не тянуть Microsoft.AspNetCore.Identity.IdentityResult
    // напрямую в контракт службы.
    public class IdentityResultWrapper
    {
        public bool Succeeded { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
        public string? UserId { get; set; }

        public static IdentityResultWrapper Success(string? userId = null) => new() { Succeeded = true, UserId = userId };
        public static IdentityResultWrapper Fail(IEnumerable<string> errors) =>
            new() { Succeeded = false, Errors = errors };
    }
}
