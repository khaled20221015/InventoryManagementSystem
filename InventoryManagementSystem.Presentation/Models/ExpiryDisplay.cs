using InventoryManagementSystem.Business.Rules;

namespace InventoryManagementSystem.Presentation.Models
{
    public static class ExpiryDisplay
    {
        public static string BadgeCss(ExpiryStatus status) => status switch
        {
            ExpiryStatus.Expired or ExpiryStatus.Today => "badge bg-danger",
            ExpiryStatus.Warning => "badge bg-warning text-dark",
            _ => "badge bg-light text-muted border"
        };

        public static string TextCss(ExpiryStatus status) => status switch
        {
            ExpiryStatus.Expired or ExpiryStatus.Today => "text-danger fw-semibold",
            ExpiryStatus.Warning => "text-warning fw-semibold",
            _ => "text-muted"
        };

        public static string Label(DateTime? expiryDate, DateTime? today = null)
        {
            var days = ExpiryRules.DaysLeft(expiryDate, today);

            if (days is null)
            {
                return "Does not expire";
            }

            if (days < 0)
            {
                return $"Expired {-days.Value} days ago";
            }

            if (days == 0)
            {
                return "Expires today";
            }

            return days.Value == 1 ? "1 day left" : $"{days.Value} days left";
        }
    }
}
