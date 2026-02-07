using CulinaryRecipes.API.Repositories.Messaging;

namespace CulinaryRecipes.API.UnitOfWork.Messaging
{
    public class MessagingUnitOfWork : IMessagingUnitOfWork
    {
        public MessagingUnitOfWork(
            IConversationRepository conversations,
            IChatMessageRepository messages,
            IMessageRequestRepository messageRequests,
            INotificationRepository notifications)
        {
            Conversations = conversations;
            Messages = messages;
            MessageRequests = messageRequests;
            Notifications = notifications;
        }

        public IConversationRepository Conversations { get; }
        public IChatMessageRepository Messages { get; }
        public IMessageRequestRepository MessageRequests { get; }
        public INotificationRepository Notifications { get; }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
