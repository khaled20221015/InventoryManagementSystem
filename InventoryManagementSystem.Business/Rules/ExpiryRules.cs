namespace InventoryManagementSystem.Business.Rules
{
    public enum ExpiryStatus{ NoExpiry , Expired , Today , Warning , Safe }

    public static class ExpiryRules
    {
        public const int WarningDays = 7;

        public const int NotifyDaysAhead = 2;

        public static int? DaysLeft(DateTime? expiryDate, DateTime? today = null)
        {
            if (expiryDate is null)
            {
                return null;
            }
            return (expiryDate.Value.Date - (today?.Date ?? DateTime.Now.Date)).Days;
        }

        public static ExpiryStatus Status(DateTime? expiryDate, DateTime? today = null)
        {
            var days = DaysLeft(expiryDate, today);

            if (days is null)
            {
                return ExpiryStatus.NoExpiry;
            }

            if (days < 0)
            {
                return ExpiryStatus.Expired;
            }

            if (days == 0)
            {
                return ExpiryStatus.Today;
            }

            if (days <= WarningDays)
            {
                return ExpiryStatus.Warning;
            }

            return ExpiryStatus.Safe;
        }

        public static bool NeedsAttention(DateTime? expiryDate, DateTime? today = null)
        {
            var status = Status(expiryDate, today);

            return status == ExpiryStatus.Expired || status == ExpiryStatus.Today || status == ExpiryStatus.Warning;
        }

        public static bool NeedsNotification(DateTime? expiryDate, DateTime? today = null)
        {
            var days = DaysLeft(expiryDate, today);

            return days is not null && days >= 0 && days <= NotifyDaysAhead;
        }

        public static bool IsTooCloseToAccept(DateTime? expiryDate, DateTime? today = null)
        {
            var days = DaysLeft(expiryDate, today);

            return days is not null && days <= NotifyDaysAhead;
        }
    }
}
