using InventoryManagementSystem.Business.DTOs;
using InventoryManagementSystem.DataAccess.Models;
using InventoryManagementSystem.DataAccess.Repositories;

namespace InventoryManagementSystem.Business.Services
{
    public class StockService
    {
        private readonly StockTransactionRepository _transactionRepository;
        private readonly ProductRepository _productRepository;

        public StockService(StockTransactionRepository transactionRepository, ProductRepository productRepository)
        {
            _transactionRepository = transactionRepository;
            _productRepository = productRepository;
        }

        public async Task<List<StockTransaction>> GetAllAsync()
        {
            return await _transactionRepository.GetAllAsync();
        }

        public async Task<string?> AddAsync(StockFormDto dto, string userId)
        {
            var product = await _productRepository.GetByIdAsync(dto.ProductId);

            if (product is null)
            {
                return "The selected product does not exist.";
            }

            if (dto.TransactionType == TransactionTypes.Out && dto.Quantity > product.StockQuantity)
            {
                return $"Not enough stock. Available quantity: {product.StockQuantity}.";
            }

            if (dto.TransactionType == TransactionTypes.In)
            {
                product.StockQuantity += dto.Quantity;
            }
            else
            {
                product.StockQuantity -= dto.Quantity;
            }

            var transaction = new StockTransaction
            {
                ProductId = product.Id,
                UserId = userId,
                Quantity = dto.Quantity,
                TransactionType = dto.TransactionType,
                TransactionDate = DateTime.Now
            };

            await _transactionRepository.AddAsync(transaction, product);

            return null;
        }
    }
}
