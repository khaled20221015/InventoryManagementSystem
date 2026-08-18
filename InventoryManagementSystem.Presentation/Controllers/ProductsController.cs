using InventoryManagementSystem.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Presentation.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();

            return View(products);
        }
    }
}