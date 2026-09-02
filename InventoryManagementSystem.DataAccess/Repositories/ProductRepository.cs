using InventoryManagementSystem.DataAccess.Data;
using InventoryManagementSystem.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.DataAccess.Repositories
{
    public class ProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync(string? search = null, int? categoryId = null)
        {
            return await BuildQuery(search, categoryId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        private IQueryable<Product> BuildQuery(string? search, int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            return query;
        }

        public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
            string? search, int? categoryId, int skip, int take)
        {
            var query = BuildQuery(search, categoryId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> NameExistsAsync(string name, int excludeId)
        {
            return await _context.Products.AnyAsync(p => p.Name == name && p.Id != excludeId);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
