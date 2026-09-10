using BlogApp.Models;
using BlogApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(ICommentService commentService, UserManager<ApplicationUser> userManager)
        {
            _commentService = commentService;
            _userManager = userManager;
        }

        // GET: /Comment  -- все комментарии (администратор/модератор)
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            var comments = await _commentService.GetAllCommentsAsync();
            return View(comments);
        }

        // GET: /Comment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: /Comment/Create  -- добавление комментария под статьёй
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int articleId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["CommentError"] = "Комментарий не может быть пустым";
                return RedirectToAction("Details", "Article", new { id = articleId });
            }

            var userId = _userManager.GetUserId(User)!;
            await _commentService.CreateCommentAsync(content, articleId, userId);

            return RedirectToAction("Details", "Article", new { id = articleId });
        }

        // POST: /Comment/Edit/5  -- автор или модератор/администратор
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            if (!CanModify(comment.UserId))
            {
                return Forbid();
            }

            await _commentService.UpdateCommentAsync(id, content);
            return RedirectToAction("Details", "Article", new { id = comment.ArticleId });
        }

        // POST: /Comment/Delete/5  -- автор или модератор/администратор
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            if (!CanModify(comment.UserId))
            {
                return Forbid();
            }

            var articleId = comment.ArticleId;
            await _commentService.DeleteCommentAsync(id);
            return RedirectToAction("Details", "Article", new { id = articleId });
        }

        private bool CanModify(string authorUserId)
        {
            var currentId = _userManager.GetUserId(User);
            return currentId == authorUserId || User.IsInRole("Admin") || User.IsInRole("Moderator");
        }
    }
}
