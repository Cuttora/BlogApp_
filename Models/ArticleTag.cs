namespace BlogApp.Models
{
    // Промежуточная таблица связи многие-ко-многим Article <-> Tag
    // Составной ключ (ArticleId, TagId) настраивается в ApplicationDbContext.
    public class ArticleTag
    {
        public int ArticleId { get; set; }
        public Article? Article { get; set; }

        public int TagId { get; set; }
        public Tag? Tag { get; set; }
    }
}
