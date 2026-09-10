using BlogApp.Models;

namespace BlogApp.Services.Interfaces
{
    public interface ITagService
    {
        Task<List<Tag>> GetAllTagsAsync();
        Task<Tag?> GetTagByIdAsync(int id);
        Task<Tag> CreateTagAsync(string name, string createdByUserId);

        // Возвращает false, если тег не найден ИЛИ если новое имя уже
        // занято другим тегом (уникальность имени тега).
        Task<bool> UpdateTagAsync(int id, string name);
        Task<bool> DeleteTagAsync(int id);
    }
}
