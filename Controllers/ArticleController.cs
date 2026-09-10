using BlogApp.Models.ViewModels;
using BlogApp.Services;
using BlogApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserActionLogger _userActionLogger;
        private readonly ILogger<ArticleController> _logger;

        public ArticleController(
            IArticleService articleService,
            UserManager<ApplicationUser> userManager,
            UserActionLogger userActionLogger,
            ILogger<ArticleController> logger)
        {
            _articleService = articleService;
            _userManager = userManager;
            _userActionLogger = userActionLogger;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var articles = await _articleService.GetAllArticlesAsync();
            return View(articles);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                _logger.LogWarning("Статья с Id={Id} не найдена", id);
                return NotFound();
            }

            return View(article);
        }

        [Authorize]
        public IActionResult Create() => View(new ArticleFormViewModel());

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;
            var userName = User.Identity?.Name ?? userId;
            var article = await _articleService.CreateArticleAsync(model.Title, model.Content, userId, model.TagsInput);

            _userActionLogger.LogAction(userName, "Создал статью", $"Id={article.Id}, Title={article.Title}");

            return RedirectToAction(nameof(Details), new { id = article.Id });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                _logger.LogWarning("Статья с Id={Id} не найдена при открытии формы редактирования", id);
                return NotFound();
            }

            if (!CanModify(article.AuthorId))
            {
                return Forbid();
            }

            var model = new ArticleFormViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                TagsInput = string.Join(", ", article.ArticleTags.Select(at => at.Tag!.Name))
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArticleFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                _logger.LogWarning("Статья с Id={Id} не найдена при попытке редактирования", id);
                return NotFound();
            }

            if (!CanModify(article.AuthorId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _articleService.UpdateArticleAsync(id, model.Title, model.Content, model.TagsInput);

            var userName = User.Identity?.Name ?? "unknown";
            _userActionLogger.LogAction(userName, "Отредактировал статью", $"Id={id}, Title={model.Title}");

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                _logger.LogWarning("Статья с Id={Id} не найдена при открытии формы удаления", id);
                return NotFound();
            }

            if (!CanModify(article.AuthorId))
            {
                return Forbid();
            }

            return View(article);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                _logger.LogWarning("Статья с Id={Id} не найдена при попытке удаления", id);
                return NotFound();
            }

            if (!CanModify(article.AuthorId))
            {
                return Forbid();
            }

            var title = article.Title;
            await _articleService.DeleteArticleAsync(id);

            var userName = User.Identity?.Name ?? "unknown";
            _userActionLogger.LogAction(userName, "Удалил статью", $"Id={id}, Title={title}");

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetByAuthor(string authorId)
        {
            var articles = await _articleService.GetArticlesByAuthorAsync(authorId);
            ViewBag.AuthorId = authorId;
            return View("Index", articles);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Search(string? query, string? tagsInput)
        {
            var model = new ArticleSearchViewModel { Query = query, TagsInput = tagsInput };

            var tagNames = string.IsNullOrWhiteSpace(tagsInput)
                ? null
                : tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            if (!string.IsNullOrWhiteSpace(query) || (tagNames != null && tagNames.Count > 0))
            {
                model.Results = await _articleService.SearchAsync(query, tagNames);
            }

            return View(model);
        }

        private bool CanModify(string authorId)
        {
            var currentId = _userManager.GetUserId(User);
            return currentId == authorId || User.IsInRole("Admin") || User.IsInRole("Moderator");
        }
    }
}