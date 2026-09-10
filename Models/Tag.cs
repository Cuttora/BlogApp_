using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogApp.Models
{
    // Тег статьи
    public class Tag
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название тега")]
        [StringLength(50, ErrorMessage = "Название тега не должно превышать 50 символов")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(CreatedByUserId))]
        public ApplicationUser? CreatedByUser { get; set; }

        public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
    }
}
