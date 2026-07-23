using BusinessLayer.Service.AI;
using Xunit;

namespace BusinessLayer.Tests.AI;

public class AiConversationStoreTests
{
    [Fact]
    public void AddMessage_TrimsOldMessagesToConfiguredLimit()
    {
        var store = new AiConversationStore();
        var conversationId = store.GetOrCreateConversationId(null);

        store.AddMessage(conversationId, "user", "one", 3);
        store.AddMessage(conversationId, "assistant", "two", 3);
        store.AddMessage(conversationId, "user", "three", 3);
        store.AddMessage(conversationId, "assistant", "four", 3);

        var messages = store.GetMessages(conversationId);

        Assert.Equal(3, messages.Count);
        Assert.DoesNotContain(messages, m => m.Content == "one");
        Assert.Equal("two", messages[0].Content);
        Assert.Equal("four", messages[^1].Content);
    }
}
