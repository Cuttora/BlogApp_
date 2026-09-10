using Microsoft.AspNetCore.Identity;

namespace BlogApp.Models
{
    // Пользователь блога. Наследуется от IdentityUser, который уже содержит
    // Id, UserName, Email, PasswordHash и т.д.
    public class ApplicationUser : IdentityUser
    {
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public string? AvatarUrl { get; set; }

        // Навигационные свойства
        public ICollection<Article> Articles { get; set; } = new List<Article>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Tag> CreatedTags { get; set; } = new List<Tag>();
    }
}
