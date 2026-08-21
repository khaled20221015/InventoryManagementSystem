using InventoryManagementSystem.Business.Reports;
using InventoryManagementSystem.DataAccess.Repositories;
using QuestPDF.Fluent;

namespace InventoryManagementSystem.Business.Services
{
    // Reads the data and hands it to the PDF document, then returns the finished file bytes.
    public class ReportService
    {
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;

        public ReportService(ProductRepository productRepository, CategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<byte[]> GenerateInventoryPdfAsync(string generatedBy)
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();

            var document = new InventoryReportDocument(products, categories.Count, generatedBy);

            return document.GeneratePdf();
        }
    }
}
