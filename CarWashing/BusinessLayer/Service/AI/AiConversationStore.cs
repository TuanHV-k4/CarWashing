using System.Collections.Concurrent;

namespace BusinessLayer.Service.AI
{
    public class AiConversationStore
    {
        private readonly ConcurrentDictionary<string, List<(string Role, string Content)>> _conversations = new();

        public string GetOrCreateConversationId(string? conversationId)
        {
            if (!string.IsNullOrWhiteSpace(conversationId) && _conversations.ContainsKey(conversationId))
                return conversationId;

            return Guid.NewGuid().ToString("N");
        }

        public List<(string Role, string Content)> GetMessages(string conversationId)
        {
            return _conversations.TryGetValue(conversationId, out var messages)
                ? messages.ToList()
                : [];
        }

        public void AddMessage(string conversationId, string role, string content, int maxMessages)
        {
            var list = _conversations.GetOrAdd(conversationId, _ => []);
            lock (list)
            {
                list.Add((role, content));
                while (list.Count > maxMessages)
                    list.RemoveAt(0);
            }
        }
    }
}
