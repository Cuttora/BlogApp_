using BlogApp.Models;
using BlogApp.Models.ViewModels;
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

        public TagController(ITagService tagService, UserManager<ApplicationUser> userManager)
        {
            _tagService = tagService;
            _userManager = userManager;
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
                return NotFound();
            }

            return View(tag);
        }

        // GET: /Tag/Create -- любой авторизованный пользователь
        [Authorize]
        public IActionResult Create() => View(new TagFormViewModel());

        // POST: /Tag/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TagFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;
            var tag = await _tagService.CreateTagAsync(model.Name, userId);
            return RedirectToAction(nameof(Details), new { id = tag.Id });
        }

        // GET: /Tag/Edit/5  -- только создатель тега или администратор
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            if (!CanModify(tag.CreatedByUserId))
            {
                return Forbid();
            }

            return View(new TagFormViewModel { Id = tag.Id, Name = tag.Name });
        }

        // POST: /Tag/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TagFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            if (!CanModify(tag.CreatedByUserId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var updated = await _tagService.UpdateTagAsync(id, model.Name);
            if (!updated)
            {
                ModelState.AddModelError(nameof(model.Name), "Тег с таким названием уже существует");
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Tag/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
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
                return NotFound();
            }

            if (!CanModify(tag.CreatedByUserId))
            {
                return Forbid();
            }

            await _tagService.DeleteTagAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private bool CanModify(string createdByUserId)
        {
            var currentId = _userManager.GetUserId(User);
            return currentId == createdByUserId || User.IsInRole("Admin");
        }
    }
}
