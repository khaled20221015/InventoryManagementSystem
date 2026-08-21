using System.Diagnostics;
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

            var model = new DashboardViewModel
            {
                TotalProducts = products.Count,
                TotalCategories = categories.Count,
                LowStockProducts = products
                    .Where(p => p.StockQuantity <= p.MinimumStockLevel)
                    .ToList()
            };

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Privacy() => View();

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}