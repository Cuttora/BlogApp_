using BlogApp.Models;

namespace BlogApp.Services.Interfaces
{
    public interface ICommentService
    {
        Task<List<Comment>> GetAllCommentsAsync();
        Task<Comment?> GetCommentByIdAsync(int id);
        Task<List<Comment>> GetCommentsByArticleAsync(int articleId);
        Task<Comment> CreateCommentAsync(string content, int articleId, string userId);
        Task<bool> UpdateCommentAsync(int id, string content);
        Task<bool> DeleteCommentAsync(int id);
    }
}
