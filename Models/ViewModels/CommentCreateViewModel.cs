using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models.ViewModels
{
    public class CommentCreateViewModel
    {
        public int ArticleId { get; set; }

        [Required(ErrorMessage = "Введите текст комментария")]
        [StringLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов")]
        [Display(Name = "Комментарий")]
        public string Content { get; set; } = string.Empty;
    }
}