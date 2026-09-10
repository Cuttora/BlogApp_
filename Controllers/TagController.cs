using BlogApp.Models;
using BlogApp.Services;
using BlogApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    public class TagController : Controller
    {
        private readonly ITagService _tagService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserActionLogger _userActionLogger;
        private readonly ILogger<TagController> _logger;

        public TagController(
            ITagService tagService,
            UserManager<ApplicationUser> userManager,
            UserActionLogger userActionLogger,
            ILogger<TagController> logger)
        {
            _tagService = tagService;
            _userManager = userManager;
            _userActionLogger = userActionLogger;
            _logger = logger;
        }

        // GET: /Tag  -- публичная страница
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return View(tags);
        }

        // GET: /Tag/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                _logger.LogWarning("Тег с Id={Id} не найден", id);
                return NotFound();
            }

            return View(tag);
        }

        // GET: /Tag/Create -- любой авторизованный пользователь
        [Authorize]
        public IActionResult Create() => View();

        // POST: /Tag/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(nameof(name), "Введите название тега");
                return View();
            }

            var userId = _userManager.GetUserId(User)!;
            var userName = User.Identity?.Name ?? userId;
            var tag = await _tagService.CreateTagAsync(name, userId);

            _userActionLogger.LogAction(userName, "Создал тег",
                $"Id={tag.Id}, Name={tag.Name}");

            return RedirectToAction(nameof(Details), new { id = tag.Id });
        }

        // GET: /Tag/Edit/5  -- только создатель тега или администратор
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                _logger.LogWarning("Тег с Id={Id} не найден при открытии формы редактирования", id);
                return NotFound();
            }

            if (!CanModify(tag.CreatedByUserId))
            {
                return Forbid();
            }

            return View(tag);
        }

        // POST: /Tag/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string name)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                _logger.LogWarning("Тег с Id={Id} не найден при попытке редактирования", id);
                return NotFound();
            }

            if (!CanModify(tag.CreatedByUserId))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(nameof(name), "Введите название тега");
                return View(tag);
            }

            await _tagService.UpdateTagAsync(id, name);

            var userName = User.Identity?.Name ?? "unknown";
            _userActionLogger.LogAction(userName, "Отредактировал тег",
                $"Id={id}, NewName={name}");

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Tag/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                _logger.LogWarning("Тег с Id={Id} не найден при открытии формы удаления", id);
                return NotFound();
            }

            if (!CanModify(tag.CreatedByUserId))
            {
                return Forbid();
            }

            return View(tag);
        }

        // POST: /Tag/Delete/5  -- только создатель или администратор
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                _logger.LogWarning("Тег с Id={Id} не найден при попытке удаления", id);
                return NotFound();
            }

            if (!CanModify(tag.CreatedByUserId))
            {
                return Forbid();
            }

            var tagName = tag.Name;
            await _tagService.DeleteTagAsync(id);

            var userName = User.Identity?.Name ?? "unknown";
            _userActionLogger.LogAction(userName, "Удалил тег",
                $"Id={id}, Name={tagName}");

            return RedirectToAction(nameof(Index));
        }

        private bool CanModify(string createdByUserId)
        {
            var currentId = _userManager.GetUserId(User);
            return currentId == createdByUserId || User.IsInRole("Admin");
        }
    }
}