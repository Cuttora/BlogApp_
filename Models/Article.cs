using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogApp.Models
{
    // Статья блога
    public class Article
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите заголовок статьи")]
        [StringLength(200, ErrorMessage = "Заголовок не должен превышать 200 символов")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите текст статьи")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public string AuthorId { get; set; } = string.Empty;

        [ForeignKey(nameof(AuthorId))]
        public ApplicationUser? Author { get; set; }

        public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
