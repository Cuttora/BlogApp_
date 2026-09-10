using BlogApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        // GET: /api/tags
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return Ok(tags);
        }

        // GET: /api/tags/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            return tag == null ? NotFound() : Ok(tag);
        }

        // POST: /api/tags
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTagDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var tag = await _tagService.CreateTagAsync(dto.Name, dto.CreatedByUserId);
            return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
        }

        // PUT: /api/tags/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTagDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _tagService.UpdateTagAsync(id, dto.Name);
            return updated ? NoContent() : NotFound();
        }

        // DELETE: /api/tags/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _tagService.DeleteTagAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }

    // DTO-модели
    public record CreateTagDto(string Name, string CreatedByUserId);
    public record UpdateTagDto(string Name);
}