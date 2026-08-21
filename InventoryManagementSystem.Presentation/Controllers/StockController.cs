using InventoryManagementSystem.Business.DTOs;
using InventoryManagementSystem.Business.Services;
using InventoryManagementSystem.DataAccess.Identity;
using InventoryManagementSystem.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagementSystem.Presentation.Controllers
{


    [Authorize]
    public class StockController : Controller
    {
        private readonly StockService _stockService;
        private readonly ProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;

        public StockController(
            StockService stockService,
            ProductService productService,
            UserManager<ApplicationUser> userManager)
        {
            _stockService = stockService;
            _productService = productService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var transactions = await _stockService.GetAllAsync();

            return View(transactions);
        }

        public async Task<IActionResult> Create()
        {
            await LoadProductsAsync();

            return View(new StockFormDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockFormDto model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User)!;
                var error = await _stockService.AddAsync(model, userId);

                if (error is null)
                {
                    TempData["Success"] = "Stock movement was recorded.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, error);
            }

            await LoadProductsAsync(model.ProductId);

            return View(model);
        }

        private async Task LoadProductsAsync(int? selectedId = null)
        {
            var products = await _productService.GetAllAsync();

            ViewBag.Products = new SelectList(products, nameof(Product.Id), nameof(Product.Name), selectedId);
        }
    }
}