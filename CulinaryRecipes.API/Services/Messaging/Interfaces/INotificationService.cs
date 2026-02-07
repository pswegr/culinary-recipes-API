using CulinaryRecipes.API.Models.Messaging;

namespace CulinaryRecipes.API.Services.Messaging.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> CreateAsync(
            string recipientUserId,
            string actorUserId,
            NotificationType type,
            string message,
            string referenceId,
            Dictionary<string, string>? metadata = null);

        Task<List<Notification>> GetForUserAsync(string userId, bool unreadOnly, int take);
        Task<long> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(string userId, string notificationId);
    }
}
