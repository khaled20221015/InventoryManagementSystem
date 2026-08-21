using InventoryManagementSystem.Business.DTOs;
using InventoryManagementSystem.DataAccess.Models;
using InventoryManagementSystem.DataAccess.Repositories;

namespace InventoryManagementSystem.Business.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoryService(CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<CategoryFormDto?> GetForEditAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
            {
                return null;
            }

            return new CategoryFormDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<string?> CreateAsync(CategoryFormDto dto)
        {
            if (await _categoryRepository.NameExistsAsync(dto.Name.Trim(), 0))
            {
                return "A category with the same name already exists.";
            }

            var category = new Category
            {
                Name = dto.Name.Trim(),
                Description = dto.Description
            };

            await _categoryRepository.AddAsync(category);

            return null;
        }

        public async Task<string?> UpdateAsync(CategoryFormDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.Id);

            if (category is null)
            {
                return "Category not found.";
            }

            if (await _categoryRepository.NameExistsAsync(dto.Name.Trim(), dto.Id))
            {
                return "A category with the same name already exists.";
            }

            category.Name = dto.Name.Trim();
            category.Description = dto.Description;

            await _categoryRepository.UpdateAsync(category);

            return null;
        }

        public async Task<string?> DeleteAsync(Category category)
        {
            if (await _categoryRepository.HasProductsAsync(category.Id))
            {
                return "This category still has products and cannot be deleted.";
            }

            await _categoryRepository.DeleteAsync(category);

            return null;
        }
    }
}