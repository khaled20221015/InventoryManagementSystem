using InventoryManagementSystem.Business.DTOs;
using InventoryManagementSystem.Business.Rules;
using InventoryManagementSystem.DataAccess.Models;
using InventoryManagementSystem.DataAccess.Repositories;

namespace InventoryManagementSystem.Business.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;

        public ProductService(ProductRepository productRepository, CategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public const int PageSize = 4;

        public async Task<List<Product>> GetAllAsync(string? search = null, int? categoryId = null)
        {
            return await _productRepository.GetAllAsync(search, categoryId);
        }

        public async Task<PagedResult<Product>> GetPageAsync(string? search, int? categoryId, int page)
        {
            if (page < 1)
            {
                page = 1;
            }

            var (items, totalCount) = await _productRepository.GetPagedAsync(
                search, categoryId, (page - 1) * PageSize, PageSize);

            var result = new PagedResult<Product>
            {
                Items = items,
                Page = page,
                PageSize = PageSize,
                TotalCount = totalCount
            };

            if (items.Count == 0 && totalCount > 0 && page > result.TotalPages)
            {
                return await GetPageAsync(search, categoryId, result.TotalPages);
            }

            return result;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<ProductFormDto?> GetForEditAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product is null)
            {
                return null;
            }

            return new ProductFormDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                MinimumStockLevel = product.MinimumStockLevel,
                ExpiryDate = product.ExpiryDate,
                CategoryId = product.CategoryId
            };
        }

        public async Task<string?> CreateAsync(ProductFormDto dto)
        {
            var error = await ValidateAsync(dto) ?? ValidateExpiry(dto.ExpiryDate);

            if (error is not null)
            {
                return error;
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                MinimumStockLevel = dto.MinimumStockLevel,
                ExpiryDate = dto.ExpiryDate,
                CategoryId = dto.CategoryId
            };

            await _productRepository.AddAsync(product);

            return null;
        }

        public async Task<string?> UpdateAsync(ProductFormDto dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.Id);

            if (product is null)
            {
                return "Product not found.";
            }

            var error = await ValidateAsync(dto);

            if (error is null && dto.ExpiryDate != product.ExpiryDate)
            {
                error = ValidateExpiry(dto.ExpiryDate);
            }

            if (error is not null)
            {
                return error;
            }

            product.Name = dto.Name.Trim();
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.MinimumStockLevel = dto.MinimumStockLevel;
            product.ExpiryDate = dto.ExpiryDate;
            product.CategoryId = dto.CategoryId;

            await _productRepository.UpdateAsync(product);

            return null;
        }

        public async Task DeleteAsync(Product product)
        {
            await _productRepository.DeleteAsync(product);
        }

        private static string? ValidateExpiry(DateTime? expiryDate)
        {
            if (!ExpiryRules.IsTooCloseToAccept(expiryDate))
            {
                return null;
            }

            var days = ExpiryRules.DaysLeft(expiryDate)!.Value;

            var state = days < 0 ? "has already passed"
                      : days == 0 ? "is today"
                      : days == 1 ? "is tomorrow"
                      : $"is only {days} days away";

            return $"The expiry date {state}. A product must have more than "
                 + $"{ExpiryRules.NotifyDaysAhead} days of shelf life left to be accepted into stock.";
        }

        private async Task<string?> ValidateAsync(ProductFormDto dto)
        {
            if (await _categoryRepository.GetByIdAsync(dto.CategoryId) is null)
            {
                return "The selected category does not exist.";
            }

            if (await _productRepository.NameExistsAsync(dto.Name.Trim(), dto.Id))
            {
                return "A product with the same name already exists.";
            }

            return null;
        }
    }
}
