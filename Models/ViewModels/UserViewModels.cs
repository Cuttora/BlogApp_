using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models.ViewModels
{
    // Отдельная модель для редактирования профиля - защищает от overposting:
    // через форму нельзя случайно/умышленно изменить PasswordHash, роли,
    // LockoutEnd и другие служебные поля IdentityUser, которые не показаны
    // на форме, но были бы доступны для записи, если бы контроллер напрямую
    // связывал модель ApplicationUser.
    public class UserEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите имя пользователя")]
        [Display(Name = "Имя пользователя")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите e-mail")]
        [EmailAddress(ErrorMessage = "Некорректный e-mail")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Ссылка на аватар")]
        public string? AvatarUrl { get; set; }

        public DateTime RegisteredAt { get; set; }
    }
}
