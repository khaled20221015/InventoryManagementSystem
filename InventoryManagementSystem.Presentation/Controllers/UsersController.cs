using InventoryManagementSystem.DataAccess.Identity;
using InventoryManagementSystem.Presentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Presentation.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var model = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "-"
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string id, string role)
        {
            if (role != RoleNames.Admin && role != RoleNames.Employee)
            {
                TempData["Error"] = "Invalid role.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "You cannot change your own role.";
                return RedirectToAction(nameof(Index));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            TempData["Success"] = $"{user.FullName} is now {role}.";

            return RedirectToAction(nameof(Index));
        }
    }
}