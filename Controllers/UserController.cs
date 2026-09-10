using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services;
using BlogApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserActionLogger _userActionLogger;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            UserManager<ApplicationUser> userManager,
            UserActionLogger userActionLogger,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _userManager = userManager;
            _userActionLogger = userActionLogger;
            _logger = logger;
        }

        // GET: /User  -- список всех пользователей (администратор или модератор)
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        // GET: /User/Details/5
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Пользователь с Id={Id} не найден", id);
                return NotFound();
            }

            return View(user);
        }

        // GET: /User/Edit/5  -- только свои данные или администратор
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (!IsSelfOrAdmin(id))
            {
                return Forbid();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Пользователь с Id={Id} не найден при открытии формы редактирования", id);
                return NotFound();
            }

            return View(user);
        }

        // POST: /User/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser model)
        {
            if (!IsSelfOrAdmin(id))
            {
                return Forbid();
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Новая сигнатура: UpdateUserAsync(id, userName, email, avatarUrl)
            var result = await _userService.UpdateUserAsync(
                model.Id,
                model.UserName ?? string.Empty,
                model.Email ?? string.Empty,
                model.AvatarUrl);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            var currentUserName = User.Identity?.Name ?? "unknown";
            _userActionLogger.LogAction(currentUserName, "Отредактировал профиль пользователя",
                $"TargetUserId={id}, UserName={model.UserName}, Email={model.Email}");

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /User/Delete/5  -- только администратор
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Пользователь с Id={Id} не найден при попытке удаления", id);
                return NotFound();
            }

            var deletedUserName = user.UserName;
            var deletedEmail = user.Email;

            var result = await _userService.DeleteUserAsync(id);
            if (!result.Succeeded)
            {
                _logger.LogError("Не удалось удалить пользователя {Id}: {Errors}",
                    id, string.Join("; ", result.Errors));
                return BadRequest(result.Errors);
            }

            var currentUserName = User.Identity?.Name ?? "unknown";
            _userActionLogger.LogAction(currentUserName, "Удалил пользователя",
                $"TargetUserId={id}, UserName={deletedUserName}, Email={deletedEmail}");

            return RedirectToAction(nameof(Index));
        }

        private bool IsSelfOrAdmin(string userId)
        {
            var currentId = _userManager.GetUserId(User);
            return currentId == userId || User.IsInRole("Admin");
        }
    }
}