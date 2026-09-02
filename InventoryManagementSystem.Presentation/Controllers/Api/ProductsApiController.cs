using InventoryManagementSystem.Business.DTOs;
using InventoryManagementSystem.Business.Services;
using InventoryManagementSystem.DataAccess.Identity;
using InventoryManagementSystem.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Presentation.Controllers.Api
{
    [ApiController]
    [Route("api/products")]
    [Authorize]
    public class ProductsApiController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsApiController(ProductService productService)
        {
            _productService = productService;
        }

        // GET /api/products?search=mouse&categoryId=2
        [HttpGet]
        public async Task<ActionResult<List<ProductResponseDto>>> GetAll(string? search, int? categoryId)
        {
            var products = await _productService.GetAllAsync(search, categoryId);

            return Ok(products.Select(ToDto).ToList());
        }

        // GET /api/products/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductResponseDto>> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            return Ok(ToDto(product));
        }

        // POST /api/products
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> Create(ProductFormDto dto)
        {
            var error = await _productService.CreateAsync(dto);

            if (error is not null)
            {
                return BadRequest(new { message = error });
            }

            return StatusCode(StatusCodes.Status201Created);
        }

        // PUT /api/products/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> Update(int id, ProductFormDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "The id in the route does not match the body." });
            }

            var error = await _productService.UpdateAsync(dto);

            if (error is not null)
            {
                return BadRequest(new { message = error });
            }

            return NoContent();
        }

        // DELETE /api/products/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            await _productService.DeleteAsync(product);

            return NoContent();
        }

        private static ProductResponseDto ToDto(Product product) => new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            MinimumStockLevel = product.MinimumStockLevel,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            IsLowStock = product.StockQuantity <= product.MinimumStockLevel
        };
    }
}
