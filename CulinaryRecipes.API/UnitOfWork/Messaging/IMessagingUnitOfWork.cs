using CulinaryRecipes.API.Repositories.Messaging;

namespace CulinaryRecipes.API.UnitOfWork.Messaging
{
    public interface IMessagingUnitOfWork
    {
        IConversationRepository Conversations { get; }
        IChatMessageRepository Messages { get; }
        IMessageRequestRepository MessageRequests { get; }
        INotificationRepository Notifications { get; }
        Task SaveChangesAsync();
    }
}
