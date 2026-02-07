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
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IMessagingUnitOfWork unitOfWork,
            IHubContext<MessagingHub, IMessagingClient> hubContext,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _logger = logger;
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
            try
            {
                return await _unitOfWork.Notifications.GetForRecipientAsync(userId, unreadOnly, take);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load notifications for user {UserId}.", userId);
                return new List<Notification>();
            }
        }

        public async Task<long> GetUnreadCountAsync(string userId)
        {
            try
            {
                return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load unread notification count for user {UserId}.", userId);
                return 0;
            }
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
