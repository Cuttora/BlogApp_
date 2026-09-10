using BlogApp.Data;
using BlogApp.Models;
using BlogApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Services
{
    public class TagService : ITagService
    {
        private readonly ApplicationDbContext _context;

        public TagService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _context.Tags
                .Include(t => t.CreatedByUser)
                .Include(t => t.ArticleTags)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Tag?> GetTagByIdAsync(int id)
        {
            return await _context.Tags
                .Include(t => t.CreatedByUser)
                .Include(t => t.ArticleTags).ThenInclude(at => at.Article)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Tag> CreateTagAsync(string name, string createdByUserId)
        {
            var normalized = name.Trim().ToLower();

            var existing = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == normalized);
            if (existing != null)
            {
                return existing;
            }

            var tag = new Tag { Name = normalized, CreatedByUserId = createdByUserId };
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task<bool> UpdateTagAsync(int id, string name)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null)
            {
                return false;
            }

            var normalized = name.Trim().ToLower();

            // Имя тега уникально (см. HasIndex(...).IsUnique() в ApplicationDbContext) -
            // проверяем коллизию заранее, чтобы вернуть понятную ошибку валидации,
            // а не падать на DbUpdateException при SaveChangesAsync.
            var duplicate = await _context.Tags.AnyAsync(t => t.Id != id && t.Name.ToLower() == normalized);
            if (duplicate)
            {
                return false;
            }

            tag.Name = normalized;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null)
            {
                return false;
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
