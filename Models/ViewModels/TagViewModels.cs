using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models.ViewModels
{
    public class TagFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название тега")]
        [StringLength(50, ErrorMessage = "Название тега не должно превышать 50 символов")]
        [Display(Name = "Название тега")]
        public string Name { get; set; } = string.Empty;
    }
}
