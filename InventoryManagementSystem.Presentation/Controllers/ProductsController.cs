using InventoryManagementSystem.Business.DTOs;
using InventoryManagementSystem.Business.Services;
using InventoryManagementSystem.DataAccess.Identity;
using InventoryManagementSystem.DataAccess.Models;
using InventoryManagementSystem.Presentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagementSystem.Presentation.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;

        public ProductsController(ProductService productService, CategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            await LoadCategoriesAsync(categoryId);

            return View(await BuildListAsync(search, categoryId, page));
        }

        public async Task<IActionResult> Table(string? search, int? categoryId, int page = 1)
        {
            return PartialView("_ProductsTable", await BuildListAsync(search, categoryId, page));
        }

        private async Task<ProductListViewModel> BuildListAsync(string? search, int? categoryId, int page)
        {
            return new ProductListViewModel
            {
                Page = await _productService.GetPageAsync(search, categoryId, page),
                Search = search,
                CategoryId = categoryId
            };
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }

        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();

            return View(new ProductFormDto());
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormDto model)
        {
            if (ModelState.IsValid)
            {
                var error = await _productService.CreateAsync(model);

                if (error is null)
                {
                    TempData["Success"] = "Product was created.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, error);
            }

            await LoadCategoriesAsync(model.CategoryId);

            return View(model);
        }

        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _productService.GetForEditAsync(id);

            if (model is null)
            {
                return NotFound();
            }

            await LoadCategoriesAsync(model.CategoryId);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductFormDto model)
        {
            if (ModelState.IsValid)
            {
                var error = await _productService.UpdateAsync(model);

                if (error is null)
                {
                    TempData["Success"] = "Product was updated.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, error);
            }

            await LoadCategoriesAsync(model.CategoryId);

            return View(model);
        }

        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = RoleNames.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            await _productService.DeleteAsync(product);

            TempData["Success"] = "Product was deleted.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategoriesAsync(int? selectedId = null)
        {
            var categories = await _categoryService.GetAllAsync();

            ViewBag.Categories = new SelectList(categories, nameof(Category.Id), nameof(Category.Name), selectedId);
        }
    }
}
