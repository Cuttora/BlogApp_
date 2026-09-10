using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models.ViewModels
{
    // Форма создания/редактирования статьи. Теги вводятся строкой через запятую,
    // например: "asp.net, ef core, sqlite" - ArticleService сам создаст
    // недостающие теги и свяжет их со статьёй.
    public class ArticleFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите заголовок статьи")]
        [StringLength(200)]
        [Display(Name = "Заголовок")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите текст статьи")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Содержание")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Теги (через запятую)")]
        public string? TagsInput { get; set; }
    }

    public class ArticleSearchViewModel
    {
        [Display(Name = "Поисковый запрос")]
        public string? Query { get; set; }

        [Display(Name = "Теги (через запятую)")]
        public string? TagsInput { get; set; }

        public List<Article> Results { get; set; } = new();
    }
}
