using CulinaryRecipes.API.Models.Messaging;

namespace CulinaryRecipes.API.Repositories.Messaging
{
    public interface IMessageRequestRepository : IMessagingGenericRepository<MessageRequest>
    {
        Task<MessageRequest?> GetPendingBetweenUsersAsync(string senderUserId, string recipientUserId);
        Task<List<MessageRequest>> GetPendingForRecipientAsync(string recipientUserId);
    }
}
