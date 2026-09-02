using Hangfire.Dashboard;
using InventoryManagementSystem.DataAccess.Identity;

namespace InventoryManagementSystem.Presentation.Jobs
{
    public class AdminOnlyDashboardFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var user = context.GetHttpContext().User;

            return user.Identity?.IsAuthenticated == true && user.IsInRole(RoleNames.Admin);
        }
    }
}
