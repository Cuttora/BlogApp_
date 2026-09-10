using BlogApp.Data;
using BlogApp.Models;
using BlogApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Services
{
    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;

        public ArticleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Article>> GetAllArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Article>> GetArticlesByAuthorAsync(string authorId)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .Where(a => a.AuthorId == authorId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Article> CreateArticleAsync(string title, string content, string authorId, string? tagsInput)
        {
            var article = new Article
            {
                Title = title,
                Content = content,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            await SyncTagsAsync(article, tagsInput, authorId);
            await _context.SaveChangesAsync();

            return article;
        }

        public async Task<bool> UpdateArticleAsync(int id, string title, string content, string? tagsInput)
        {
            var article = await _context.Articles
                .Include(a => a.ArticleTags)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return false;
            }

            article.Title = title;
            article.Content = content;
            article.UpdatedAt = DateTime.UtcNow;

            // Пересобираем связи со статьёй по новому списку тегов
            _context.ArticleTags.RemoveRange(article.ArticleTags);
            await SyncTagsAsync(article, tagsInput, article.AuthorId);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null)
            {
                return false;
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Article>> SearchAsync(string? query, List<string>? tagNames)
        {
            var articles = _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var pattern = $"%{query.Trim()}%";
                articles = articles.Where(a =>
                    EF.Functions.Like(a.Title, pattern) ||
                    EF.Functions.Like(a.Content, pattern));
            }

            if (tagNames != null && tagNames.Count > 0)
            {
                var normalizedTags = tagNames
                    .Select(t => t.Trim().ToLower())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();

                if (normalizedTags.Count > 0)
                {
                    articles = articles.Where(a =>
                        a.ArticleTags.Any(at => normalizedTags.Contains(at.Tag!.Name.ToLower())));
                }
            }

            return await articles.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        // Разбирает строку тегов вида "asp.net, ef core", находит существующие
        // теги или создаёт новые и связывает их со статьёй.
        private async Task SyncTagsAsync(Article article, string? tagsInput, string createdByUserId)
        {
            if (string.IsNullOrWhiteSpace(tagsInput))
            {
                return;
            }

            var names = tagsInput
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(n => n.ToLower())
                .Distinct()
                .ToList();

            foreach (var name in names)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == name);
                if (tag == null)
                {
                    tag = new Tag { Name = name, CreatedByUserId = createdByUserId };
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync();
                }

                _context.ArticleTags.Add(new ArticleTag { ArticleId = article.Id, TagId = tag.Id });
            }
        }
    }
}
