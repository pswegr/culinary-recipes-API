using CulinaryRecipes.API.Models.Messaging;

namespace CulinaryRecipes.API.Hubs
{
    public interface IMessagingClient
    {
        Task HandshakeAcknowledged(MessagingHandshake handshake);
        Task MessageRequestReceived(MessageRequest request);
        Task MessageRequestUpdated(MessageRequest request);
        Task MessageReceived(ChatMessage message);
        Task ConversationUpdated(Conversation conversation);
        Task MessageAlertReceived(MessageAlert alert);
        Task NotificationReceived(Notification notification);
    }
}
