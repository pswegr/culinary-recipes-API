using CulinaryRecipes.API.Hubs;
using CulinaryRecipes.API.Models.Messaging;
using CulinaryRecipes.API.Services.Messaging.Interfaces;
using CulinaryRecipes.API.UnitOfWork.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace CulinaryRecipes.API.Services.Messaging
{
    public class NotificationService : INotificationService
    {
        private readonly IMessagingUnitOfWork _unitOfWork;
        private readonly IHubContext<MessagingHub, IMessagingClient> _hubContext;

        public NotificationService(
            IMessagingUnitOfWork unitOfWork,
            IHubContext<MessagingHub, IMessagingClient> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        public async Task<Notification> CreateAsync(
            string recipientUserId,
            string actorUserId,
            NotificationType type,
            string message,
            string referenceId,
            Dictionary<string, string>? metadata = null)
        {
            var notification = new Notification
            {
                RecipientUserId = recipientUserId,
                ActorUserId = actorUserId,
                Type = type,
                Message = message,
                ReferenceId = referenceId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            await _unitOfWork.Notifications.InsertAsync(notification);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.User(recipientUserId).NotificationReceived(notification);

            return notification;
        }

        public async Task<List<Notification>> GetForUserAsync(string userId, bool unreadOnly, int take)
        {
            return await _unitOfWork.Notifications.GetForRecipientAsync(userId, unreadOnly, take);
        }

        public async Task<long> GetUnreadCountAsync(string userId)
        {
            return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
        }

        public async Task<bool> MarkAsReadAsync(string userId, string notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null || notification.RecipientUserId != userId)
            {
                return false;
            }

            if (notification.IsRead)
            {
                return true;
            }

            notification.IsRead = true;
            await _unitOfWork.Notifications.ReplaceAsync(notification.id!, notification);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
