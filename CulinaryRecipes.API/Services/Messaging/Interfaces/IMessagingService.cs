using CulinaryRecipes.API.Models.Messaging;
using CulinaryRecipes.API.Models.Messaging.Requests;

namespace CulinaryRecipes.API.Services.Messaging.Interfaces
{
    public interface IMessagingService
    {
        Task<MessagingHandshake> CreateHandshakeAsync(string userId, string connectionId);
        Task<MessageRequest?> CreateMessageRequestAsync(string senderUserId, string recipientUserId);
        Task<MessageRequest?> RespondToMessageRequestAsync(string requestId, string recipientUserId, bool accept);
        Task<ChatMessage?> SendMessageAsync(string senderUserId, SendMessageModel model);
        Task<List<MessageRequest>> GetPendingRequestsAsync(string userId);
        Task<List<Conversation>> GetConversationsAsync(string userId);
        Task<List<ChatMessage>> GetConversationMessagesAsync(string userId, string conversationId, int skip, int take);
        Task<bool> CanAccessConversationAsync(string userId, string conversationId);
    }
}
