using BlogApp.Models;

namespace BlogApp.Services.Interfaces
{
    public interface IArticleService
    {
        Task<List<Article>> GetAllArticlesAsync();
        Task<Article?> GetArticleByIdAsync(int id);
        Task<List<Article>> GetArticlesByAuthorAsync(string authorId);
        Task<Article> CreateArticleAsync(string title, string content, string authorId, string? tagsInput);
        Task<bool> UpdateArticleAsync(int id, string title, string content, string? tagsInput);
        Task<bool> DeleteArticleAsync(int id);
        Task<List<Article>> SearchAsync(string? query, List<string>? tagNames);
    }
}
