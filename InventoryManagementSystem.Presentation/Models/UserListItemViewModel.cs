namespace InventoryManagementSystem.Presentation.Models
{
    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsCurrentUser { get; set; }

        public bool IsDeleted { get; set; }
    }
}
