using BlogApp.Models.ViewModels;
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

        public ArticleController(IArticleService articleService, UserManager<ApplicationUser> userManager)
        {
            _articleService = articleService;
            _userManager = userManager;
        }

        // GET: /Article  -- список всех статей, публичная страница
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var articles = await _articleService.GetAllArticlesAsync();
            return View(articles);
        }

        // GET: /Article/Details/5  -- публичная страница
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // GET: /Article/Create  -- только авторизованный пользователь
        [Authorize]
        public IActionResult Create() => View(new ArticleFormViewModel());

        // POST: /Article/Create
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
            var article = await _articleService.CreateArticleAsync(model.Title, model.Content, userId, model.TagsInput);

            return RedirectToAction(nameof(Details), new { id = article.Id });
        }

        // GET: /Article/Edit/5  -- автор или модератор/администратор
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
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

        // POST: /Article/Edit/5
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
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Article/Delete/5 -- подтверждение удаления
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            if (!CanModify(article.AuthorId))
            {
                return Forbid();
            }

            return View(article);
        }

        // POST: /Article/Delete/5  -- автор или модератор/администратор
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            if (!CanModify(article.AuthorId))
            {
                return Forbid();
            }

            await _articleService.DeleteArticleAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Article/GetByAuthor/{authorId}  -- все статьи заданного автора
        [AllowAnonymous]
        public async Task<IActionResult> GetByAuthor(string authorId)
        {
            var articles = await _articleService.GetArticlesByAuthorAsync(authorId);
            ViewBag.AuthorId = authorId;
            return View("Index", articles);
        }

        // GET/POST: /Article/Search  -- поиск по тексту и/или тегам
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

        // Автор статьи либо модератор/администратор может её редактировать/удалять
        private bool CanModify(string authorId)
        {
            var currentId = _userManager.GetUserId(User);
            return currentId == authorId || User.IsInRole("Admin") || User.IsInRole("Moderator");
        }
    }
}
