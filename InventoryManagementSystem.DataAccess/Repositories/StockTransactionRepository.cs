using InventoryManagementSystem.DataAccess.Data;
using InventoryManagementSystem.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.DataAccess.Repositories
{
    public class StockTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public StockTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StockTransaction>> GetAllAsync()
        {
            return await _context.StockTransactions
                .Include(t => t.Product)
                .Include(t => t.User)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task AddAsync(StockTransaction transaction, Product product)
        {
            await _context.StockTransactions.AddAsync(transaction);
            _context.Products.Update(product);

            await _context.SaveChangesAsync();
        }
    }
}
