using System.Net;
using System.Text;
using InventoryManagementSystem.Business.Rules;
using InventoryManagementSystem.Business.Services;
using InventoryManagementSystem.DataAccess.Models;
using InventoryManagementSystem.Presentation.Services;
using Microsoft.Extensions.Options;

namespace InventoryManagementSystem.Presentation.Jobs
{
    public class ExpiryAlertJob
    {
        public const string RecurringJobId = "expiry-alert";

        private readonly ProductService _productService;
        private readonly EmailSender _emailSender;
        private readonly EmailSettings _settings;
        private readonly ILogger<ExpiryAlertJob> _logger;

        public ExpiryAlertJob(
            ProductService productService,
            EmailSender emailSender,
            IOptions<EmailSettings> settings,
            ILogger<ExpiryAlertJob> logger)
        {
            _productService = productService;
            _emailSender = emailSender;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            var products = await _productService.GetAllAsync();

            var expiring = products
                .Where(p => ExpiryRules.NeedsNotification(p.ExpiryDate))
                .OrderBy(p => p.ExpiryDate)
                .ToList();

            if (expiring.Count == 0)
            {
                _logger.LogInformation(
                    "Expiry alert: nothing expires within {Days} days.", ExpiryRules.NotifyDaysAhead);

                return;
            }

            _logger.LogInformation(
                "Expiry alert: {Count} product(s) expire within {Days} days.",
                expiring.Count, ExpiryRules.NotifyDaysAhead);

            var subject = $"Expiry alert: {expiring.Count} product(s) expiring within {ExpiryRules.NotifyDaysAhead} days";

            await _emailSender.SendAsync(_settings.AlertRecipient, subject, BuildBody(expiring));
        }

        private static string BuildBody(List<Product> expiring)
        {
            var html = new StringBuilder();

            html.Append(
                "<div style=\"font-family:Segoe UI,Arial,sans-serif;color:#171C26\">" +
                "<h2 style=\"margin:0 0 4px\">Expiry alert</h2>" +
                $"<p style=\"margin:0 0 16px;color:#6B7689\">These products expire within {ExpiryRules.NotifyDaysAhead} days.</p>" +
                "<table cellpadding=\"8\" cellspacing=\"0\" style=\"border-collapse:collapse;font-size:14px\">" +
                "<tr style=\"background:#EFF2F7;text-align:left\">" +
                "<th>Product</th><th>Category</th><th>Expiry date</th><th>Days left</th></tr>");

            foreach (var product in expiring)
            {
                var days = ExpiryRules.DaysLeft(product.ExpiryDate)!.Value;

                var label = days == 0 ? "Expires today"
                          : days == 1 ? "1 day left"
                          : $"{days} days left";

                html.Append(
                    "<tr style=\"border-top:1px solid #DFE4EC\">" +
                    $"<td><b>{WebUtility.HtmlEncode(product.Name)}</b></td>" +
                    $"<td>{WebUtility.HtmlEncode(product.Category?.Name ?? "-")}</td>" +
                    $"<td>{product.ExpiryDate!.Value:yyyy-MM-dd}</td>" +
                    $"<td style=\"color:#B42318;font-weight:600\">{label}</td></tr>");
            }

            html.Append(
                "</table>" +
                $"<p style=\"margin:16px 0 0;color:#6B7689;font-size:12px\">Sent by Khaled Inventory at {DateTime.Now:yyyy-MM-dd HH:mm}.</p>" +
                "</div>");

            return html.ToString();
        }
    }
}
