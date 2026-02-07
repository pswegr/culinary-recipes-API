using CulinaryRecipes.API.Models.Messaging;

namespace CulinaryRecipes.API.Repositories.Messaging
{
    public interface INotificationRepository : IMessagingGenericRepository<Notification>
    {
        Task<List<Notification>> GetForRecipientAsync(string recipientUserId, bool unreadOnly, int take);
        Task<long> GetUnreadCountAsync(string recipientUserId);
    }
}
