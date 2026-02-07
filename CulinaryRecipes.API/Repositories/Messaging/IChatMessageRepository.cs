using CulinaryRecipes.API.Models.Messaging;

namespace CulinaryRecipes.API.Repositories.Messaging
{
    public interface IChatMessageRepository : IMessagingGenericRepository<ChatMessage>
    {
        Task<List<ChatMessage>> GetForConversationAsync(string conversationId, int skip, int take);
    }
}
