using InventoryManagementSystem.Business.Services;
using InventoryManagementSystem.DataAccess.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Presentation.Controllers
{
    // Any signed-in user can download the inventory report.
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ReportService _reportService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(ReportService reportService, UserManager<ApplicationUser> userManager)
        {
            _reportService = reportService;
            _userManager = userManager;
        }

        public async Task<IActionResult> InventoryPdf()
        {
            var user = await _userManager.GetUserAsync(User);
            var generatedBy = user?.FullName ?? "Unknown user";

            var pdfBytes = await _reportService.GenerateInventoryPdfAsync(generatedBy);
            var fileName = $"inventory-report-{DateTime.Now:yyyy-MM-dd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
