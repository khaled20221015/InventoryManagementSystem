using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryManagementSystem.DataAccess.Models;
using InventoryManagementSystem.DataAccess.Repositories;

namespace InventoryManagementSystem.Business.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepository;

        public ProductService(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task AddAsync(Product product)
        {
            if (product.Price <= 0)
                throw new ArgumentException("Price must be greater than zero.");

            if (product.StockQuantity < 0)
                throw new ArgumentException("Stock cannot be negative.");

            await _productRepository.AddAsync(product);
        }
    }
}
