using BlogApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: /api/comments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var comments = await _commentService.GetAllCommentsAsync();
            return Ok(comments);
        }

        // GET: /api/comments/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            return comment == null ? NotFound() : Ok(comment);
        }

        // GET: /api/comments/article/5
        [HttpGet("article/{articleId:int}")]
        public async Task<IActionResult> GetByArticle(int articleId)
        {
            var comments = await _commentService.GetCommentsByArticleAsync(articleId);
            return Ok(comments);
        }

        // POST: /api/comments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Комментарий не может быть пустым");

            var comment = await _commentService.CreateCommentAsync(
                dto.Content, dto.ArticleId, dto.UserId);

            return CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment);
        }

        // PUT: /api/comments/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _commentService.UpdateCommentAsync(id, dto.Content);
            return updated ? NoContent() : NotFound();
        }

        // DELETE: /api/comments/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _commentService.DeleteCommentAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }

    // DTO-модели
    public record CreateCommentDto(string Content, int ArticleId, string UserId);
    public record UpdateCommentDto(string Content);
}