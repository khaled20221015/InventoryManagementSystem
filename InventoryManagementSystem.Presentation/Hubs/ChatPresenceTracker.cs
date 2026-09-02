namespace InventoryManagementSystem.Presentation.Hubs
{
    public class ChatPresenceTracker
    {
        private readonly Dictionary<string, int> _connectionsPerUser = new();

        private readonly object _gate = new();

        public bool Add(string userId)
        {
            lock (_gate)
            {
                _connectionsPerUser.TryGetValue(userId, out var count);
                _connectionsPerUser[userId] = count + 1;

                return count == 0;
            }
        }

        public bool Remove(string userId)
        {
            lock (_gate)
            {
                if (!_connectionsPerUser.TryGetValue(userId, out var count))
                {
                    return false;
                }

                if (count <= 1)
                {
                    _connectionsPerUser.Remove(userId);
                    return true;
                }

                _connectionsPerUser[userId] = count - 1;

                return false;
            }
        }

        public HashSet<string> GetOnlineUserIds()
        {
            lock (_gate)
            {
                return new HashSet<string>(_connectionsPerUser.Keys);
            }
        }
    }
}
