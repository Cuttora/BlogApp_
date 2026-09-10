using BlogApp.Models;
using BlogApp.Models.ViewModels;
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

        public UserController(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
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
                return NotFound();
            }

            var model = new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                AvatarUrl = user.AvatarUrl,
                RegisteredAt = user.RegisteredAt
            };

            return View(model);
        }

        // POST: /User/Edit/5
        // Принимает только UserEditViewModel (белый список полей), а не всю
        // сущность ApplicationUser - иначе через сырой HTTP-запрос можно было бы
        // переписать PasswordHash, роли и другие служебные поля (overposting).
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserEditViewModel model)
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

            var result = await _userService.UpdateUserAsync(id, model.UserName, model.Email, model.AvatarUrl);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /User/Delete/5  -- только администратор
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            await _userService.DeleteUserAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private bool IsSelfOrAdmin(string userId)
        {
            var currentId = _userManager.GetUserId(User);
            return currentId == userId || User.IsInRole("Admin");
        }
    }
}
