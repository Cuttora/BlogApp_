using BlogApp.Models;
using BlogApp.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(
            IUserService userService,
            UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        // GET: /api/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: /api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        // POST: /api/users
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.CreateUserAsync(dto.UserName, dto.Email, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var created = await _userService.GetUserByIdAsync(result.UserId!);
            return CreatedAtAction(nameof(GetById), new { id = result.UserId }, created);
        }

        // PUT: /api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.UpdateUserAsync(id, dto.UserName, dto.Email, dto.AvatarUrl);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }

        // DELETE: /api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _userService.GetUserByIdAsync(id);
            if (existing == null) return NotFound();

            var result = await _userService.DeleteUserAsync(id);
            return result.Succeeded ? NoContent() : BadRequest(result.Errors);
        }

        // GET: /api/users/{id}/roles
        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }
    }

    // DTO-модели
    public record CreateUserDto(string UserName, string Email, string Password);
    public record UpdateUserDto(string UserName, string Email, string? AvatarUrl);
}