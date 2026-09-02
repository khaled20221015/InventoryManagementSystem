using InventoryManagementSystem.DataAccess.Identity;
using InventoryManagementSystem.DataAccess.Models;
using InventoryManagementSystem.DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace InventoryManagementSystem.Presentation.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private const int MaxMessageLength = 500;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ChatPresenceTracker _presence;
        private readonly ChatMessageRepository _messages;

        public ChatHub(
            UserManager<ApplicationUser> userManager,
            ChatPresenceTracker presence,
            ChatMessageRepository messages)
        {
            _userManager = userManager;
            _presence = presence;
            _messages = messages;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            if (userId is not null && _presence.Add(userId))
            {
                await Clients.Others.SendAsync("UserPresenceChanged", userId, true);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;

            if (userId is not null && _presence.Remove(userId))
            {
                await Clients.Others.SendAsync("UserPresenceChanged", userId, false);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendPrivateMessage(string toUserId, string message)
        {
            message = (message ?? string.Empty).Trim();

            if (message.Length == 0 || message.Length > MaxMessageLength)
            {
                return;
            }

            var sender = await _userManager.GetUserAsync(Context.User!);

            if (sender is null || sender.IsDeleted)
            {
                return;
            }

            var receiver = await _userManager.FindByIdAsync(toUserId);

            if (receiver is null || receiver.IsDeleted)
            {
                return;
            }

            await _messages.AddAsync(new ChatMessage
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Content = message,
                SentAt = DateTime.Now
            });

            await Clients.User(toUserId).SendAsync("ReceiveMessage", sender.Id, message);
        }

        public async Task SendBroadcastMessage(string message)
        {
            message = (message ?? string.Empty).Trim();

            if (message.Length == 0 || message.Length > MaxMessageLength)
            {
                return;
            }

            var sender = await _userManager.GetUserAsync(Context.User!);

            if (sender is null || sender.IsDeleted)
            {
                return;
            }

            if (!await _userManager.IsInRoleAsync(sender, RoleNames.Admin))
            {
                return;
            }

            var recipients = (await _userManager.GetUsersInRoleAsync(RoleNames.Employee))
                .Where(e => !e.IsDeleted && e.Id != sender.Id)
                .ToList();

            if (recipients.Count == 0)
            {
                return;
            }

            await _messages.AddRangeAsync(recipients.Select(r => new ChatMessage
            {
                SenderId = sender.Id,
                ReceiverId = r.Id,
                Content = message,
                SentAt = DateTime.Now
            }));

            await Clients.Users(recipients.Select(r => r.Id).ToList())
                .SendAsync("ReceiveMessage", sender.Id, message);
        }

        public async Task MarkConversationRead(string otherUserId)
        {
            var userId = Context.UserIdentifier;

            if (userId is null || string.IsNullOrWhiteSpace(otherUserId))
            {
                return;
            }

            await _messages.MarkConversationReadAsync(userId, otherUserId);
        }
    }
}
