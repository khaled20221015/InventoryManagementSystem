using InventoryManagementSystem.DataAccess.Identity;
using InventoryManagementSystem.DataAccess.Repositories;
using InventoryManagementSystem.Presentation.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Presentation.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ChatPresenceTracker _presence;
        private readonly ChatMessageRepository _messages;

        public ChatController(
            UserManager<ApplicationUser> userManager,
            ChatPresenceTracker presence,
            ChatMessageRepository messages)
        {
            _userManager = userManager;
            _presence = presence;
            _messages = messages;
        }

        [HttpGet]
        public async Task<IActionResult> Contacts()
        {
            var currentUserId = _userManager.GetUserId(User);

            var contacts = await _userManager.Users
                .Where(u => !u.IsDeleted && u.Id != currentUserId)
                .OrderBy(u => u.FullName)
                .Select(u => new { id = u.Id, name = u.FullName })
                .ToListAsync();

            var onlineUserIds = _presence.GetOnlineUserIds();

            var unreadCounts = await _messages.GetUnreadCountsAsync(currentUserId!);

            var result = contacts.Select(c => new
            {
                c.id,
                c.name,
                isOnline = onlineUserIds.Contains(c.id),
                unread = unreadCounts.TryGetValue(c.id, out var count) ? count : 0
            });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> History(string withUserId)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId is null || string.IsNullOrWhiteSpace(withUserId))
            {
                return Json(Array.Empty<object>());
            }

            var messages = await _messages.GetConversationAsync(currentUserId, withUserId);

            var result = messages.Select(m => new
            {
                mine = m.SenderId == currentUserId,
                message = m.Content
            });

            return Json(result);
        }
    }
}
