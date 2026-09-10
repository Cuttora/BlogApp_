using BlogApp.Models;
using BlogApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticlesController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        // GET: /api/articles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Article>>> GetAll()
        {
            var articles = await _articleService.GetAllArticlesAsync();
            return Ok(articles);
        }

        // GET: /api/articles/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Article>> GetById(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null) return NotFound();
            return Ok(article);
        }

        // GET: /api/articles/author/{authorId}
        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<IEnumerable<Article>>> GetByAuthor(string authorId)
        {
            var articles = await _articleService.GetArticlesByAuthorAsync(authorId);
            return Ok(articles);
        }

        // POST: /api/articles
        [HttpPost]
        public async Task<ActionResult<Article>> Create([FromBody] CreateArticleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var article = await _articleService.CreateArticleAsync(
                dto.Title, dto.Content, dto.AuthorId, dto.TagsInput);

            return CreatedAtAction(nameof(GetById), new { id = article.Id }, article);
        }

        // PUT: /api/articles/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateArticleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _articleService.UpdateArticleAsync(
                id, dto.Title, dto.Content, dto.TagsInput);

            return updated ? NoContent() : NotFound();
        }

        // DELETE: /api/articles/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _articleService.DeleteArticleAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        // GET: /api/articles/search?query=...&tags=...
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Article>>> Search(string? query, string? tags)
        {
            var tagNames = string.IsNullOrWhiteSpace(tags)
                ? null
                : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            var results = await _articleService.SearchAsync(query, tagNames);
            return Ok(results);
        }
    }

    // DTO-модели
    public record CreateArticleDto(string Title, string Content, string AuthorId, string? TagsInput);
    public record UpdateArticleDto(string Title, string Content, string? TagsInput);
}