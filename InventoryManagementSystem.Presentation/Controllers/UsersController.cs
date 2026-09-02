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

        public async Task<IActionResult> Index(bool showDeleted = false)
        {
            var currentUserId = _userManager.GetUserId(User);

            ViewData["ShowDeleted"] = showDeleted;

            ViewData["DeletedCount"] = await _userManager.Users.CountAsync(u => u.IsDeleted);

            var users = await _userManager.Users
                .Where(u => showDeleted || !u.IsDeleted)
                .OrderBy(u => u.IsDeleted)
                .ThenBy(u => u.FullName)
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
                    Role = roles.FirstOrDefault() ?? "-",
                    IsCurrentUser = user.Id == currentUserId,
                    IsDeleted = user.IsDeleted
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

            if (user is null || user.IsDeleted)
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

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null || user.IsDeleted)
            {
                return NotFound();
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            if (await IsLastAdminAsync(user))
            {
                TempData["Error"] = "You cannot delete the last administrator.";
                return RedirectToAction(nameof(Index));
            }

            user.IsDeleted = true;

            var result = await _userManager.UpdateSecurityStampAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error"] = "The user could not be deleted.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = $"{user.FullName} was deleted.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null || !user.IsDeleted)
            {
                return NotFound();
            }

            user.IsDeleted = false;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error"] = "The user could not be restored.";
                return RedirectToAction(nameof(Index), new { showDeleted = true });
            }

            TempData["Success"] = $"{user.FullName} was restored and keeps the {await GetRoleAsync(user)} role.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GetRoleAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return roles.FirstOrDefault() ?? "-";
        }

        private async Task<bool> IsLastAdminAsync(ApplicationUser user)
        {
            if (!await _userManager.IsInRoleAsync(user, RoleNames.Admin))
            {
                return false;
            }

            var admins = await _userManager.GetUsersInRoleAsync(RoleNames.Admin);

            return admins.Count(a => !a.IsDeleted) <= 1;
        }
    }
}
