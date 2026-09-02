using InventoryManagementSystem.DataAccess.Data;
using InventoryManagementSystem.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.DataAccess.Repositories
{
    public class ChatMessageRepository
    {
        private const int DefaultHistorySize = 50;

        private readonly ApplicationDbContext _context;

        public ChatMessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);

            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<ChatMessage> messages)
        {
            await _context.ChatMessages.AddRangeAsync(messages);

            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<string, int>> GetUnreadCountsAsync(string userId)
        {
            return await _context.ChatMessages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                .Select(g => new { SenderId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SenderId, x => x.Count);
        }

        public async Task<int> MarkConversationReadAsync(string userId, string otherUserId)
        {
            return await _context.ChatMessages
                .Where(m => m.ReceiverId == userId && m.SenderId == otherUserId && !m.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.IsRead, true));
        }

        public async Task<List<ChatMessage>> GetConversationAsync(
            string userId, string otherUserId, int take = DefaultHistorySize)
        {
            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId)
                         || (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderByDescending(m => m.SentAt)
                .Take(take)
                .ToListAsync();

            messages.Reverse();

            return messages;
        }
    }
}
