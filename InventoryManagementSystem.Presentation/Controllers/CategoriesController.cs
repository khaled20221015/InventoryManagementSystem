using InventoryManagementSystem.Business.DTOs;
using InventoryManagementSystem.Business.Services;
using InventoryManagementSystem.DataAccess.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Presentation.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    public class CategoriesController : Controller
    {
        private readonly CategoryService _categoryService;

        public CategoriesController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();

            return View(categories);
        }

        public IActionResult Create() => View(new CategoryFormDto());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormDto model)
        {
            if (ModelState.IsValid)
            {
                var error = await _categoryService.CreateAsync(model);

                if (error is null)
                {
                    TempData["Success"] = "Category was created.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _categoryService.GetForEditAsync(id);

            if (model is null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryFormDto model)
        {
            if (ModelState.IsValid)
            {
                var error = await _categoryService.UpdateAsync(model);

                if (error is null)
                {
                    TempData["Success"] = "Category was updated.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            var error = await _categoryService.DeleteAsync(category);

            if (error is null)
            {
                TempData["Success"] = "Category was deleted.";
            }
            else
            {
                TempData["Error"] = error;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
