using InventoryManagementSystem.Business.Rules;
using InventoryManagementSystem.Business.Services;
using InventoryManagementSystem.Presentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Presentation.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;

        public HomeController(ProductService productService, CategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();

            var topProducts = products.OrderByDescending(p => p.StockQuantity).Take(10).ToList();

            var model = new DashboardViewModel
            {
                TotalProducts = products.Count,
                TotalCategories = categories.Count,

                LowStockProducts = products.Where(p => p.StockQuantity <= p.MinimumStockLevel).ToList(),

                ExpiringProducts = products
                    .Where(p => ExpiryRules.NeedsAttention(p.ExpiryDate))
                    .OrderBy(p => p.ExpiryDate)
                    .ToList(),

                CategoryNames = categories.Select(c => c.Name).ToList(),
                ProductsPerCategory = categories.Select(c => products.Count(p => p.CategoryId == c.Id)).ToList(),

                TopProductNames = topProducts.Select(p => p.Name).ToList(),
                TopProductQuantities = topProducts.Select(p => p.StockQuantity).ToList()
            };

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Error(int code = 500)
        {
            ViewData["Code"] = code;

            ViewData["Message"] = code switch
            {
                404 => "The page you asked for does not exist.",
                403 => "You do not have permission to open this page.",
                _ => "Something went wrong while handling your request."
            };

            return View();
        }
    }
}
