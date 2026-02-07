using CulinaryRecipes.API.Models.Messaging;
using CulinaryRecipes.API.Models.Messaging.Requests;
using CulinaryRecipes.API.Services.Messaging.Interfaces;
using CulinaryRecipes.API.UnitOfWork.Messaging;

namespace CulinaryRecipes.API.Services.Messaging
{
    public class MessagingService : IMessagingService
    {
        private readonly IMessagingUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public MessagingService(
            IMessagingUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<MessagingHandshake> CreateHandshakeAsync(string userId, string connectionId)
        {
            var pendingRequests = await _unitOfWork.MessageRequests.GetPendingForRecipientAsync(userId);
            var unreadNotifications = await _notificationService.GetUnreadCountAsync(userId);

            return new MessagingHandshake
            {
                UserId = userId,
                ConnectionId = connectionId,
                ServerTimeUtc = DateTime.UtcNow,
                PendingRequestCount = pendingRequests.Count,
                UnreadNotificationCount = (int)unreadNotifications
            };
        }

        public async Task<MessageRequest?> CreateMessageRequestAsync(string senderUserId, string recipientUserId)
        {
            if (string.IsNullOrWhiteSpace(senderUserId) ||
                string.IsNullOrWhiteSpace(recipientUserId) ||
                senderUserId == recipientUserId)
            {
                return null;
            }

            var existingConversation = await _unitOfWork.Conversations.GetByParticipantsAsync(senderUserId, recipientUserId);
            if (existingConversation != null)
            {
                return null;
            }

            var existingRequest = await _unitOfWork.MessageRequests.GetPendingBetweenUsersAsync(senderUserId, recipientUserId);
            if (existingRequest != null)
            {
                return existingRequest;
            }

            var request = new MessageRequest
            {
                SenderUserId = senderUserId,
                RecipientUserId = recipientUserId,
                CreatedAt = DateTime.UtcNow,
                Status = MessageRequestStatus.Pending
            };

            await _unitOfWork.MessageRequests.InsertAsync(request);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.CreateAsync(
                recipientUserId,
                senderUserId,
                NotificationType.MessageRequest,
                "New messaging request",
                request.id ?? string.Empty,
                new Dictionary<string, string>
                {
                    { "requestStatus", request.Status.ToString() }
                });

            return request;
        }

        public async Task<MessageRequest?> RespondToMessageRequestAsync(string requestId, string recipientUserId, bool accept)
        {
            if (string.IsNullOrWhiteSpace(requestId) || string.IsNullOrWhiteSpace(recipientUserId))
            {
                return null;
            }

            var request = await _unitOfWork.MessageRequests.GetByIdAsync(requestId);
            if (request == null || request.RecipientUserId != recipientUserId || request.Status != MessageRequestStatus.Pending)
            {
                return null;
            }

            request.Status = accept ? MessageRequestStatus.Accepted : MessageRequestStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;
            await _unitOfWork.MessageRequests.ReplaceAsync(request.id!, request);

            string conversationId = string.Empty;
            if (accept)
            {
                var existingConversation = await _unitOfWork.Conversations.GetByParticipantsAsync(request.SenderUserId, request.RecipientUserId);
                if (existingConversation == null)
                {
                    var conversation = new Conversation
                    {
                        ParticipantUserIds = new List<string> { request.SenderUserId, request.RecipientUserId },
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LastMessagePreview = string.Empty,
                        LastMessageAt = null
                    };

                    await _unitOfWork.Conversations.InsertAsync(conversation);
                    conversationId = conversation.id ?? string.Empty;
                }
                else
                {
                    conversationId = existingConversation.id ?? string.Empty;
                }
            }

            await _unitOfWork.SaveChangesAsync();

            await _notificationService.CreateAsync(
                request.SenderUserId,
                recipientUserId,
                NotificationType.MessageRequest,
                accept ? "Messaging request accepted" : "Messaging request rejected",
                request.id ?? string.Empty,
                new Dictionary<string, string>
                {
                    { "requestStatus", request.Status.ToString() },
                    { "conversationId", conversationId }
                });

            return request;
        }

        public async Task<ChatMessage?> SendMessageAsync(string senderUserId, SendMessageModel model)
        {
            if (string.IsNullOrWhiteSpace(senderUserId) || model == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(model.Content) && model.Attachments.Count == 0)
            {
                return null;
            }

            if (model.Attachments.Any(a => !IsValidAttachment(a)))
            {
                return null;
            }

            Conversation? conversation;
            if (!string.IsNullOrWhiteSpace(model.ConversationId))
            {
                conversation = await _unitOfWork.Conversations.GetByIdAsync(model.ConversationId);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.RecipientUserId))
                {
                    return null;
                }

                conversation = await _unitOfWork.Conversations.GetByParticipantsAsync(senderUserId, model.RecipientUserId);
            }

            if (conversation == null || !conversation.ParticipantUserIds.Contains(senderUserId))
            {
                return null;
            }

            var recipientUserId = !string.IsNullOrWhiteSpace(model.RecipientUserId)
                ? model.RecipientUserId
                : conversation.ParticipantUserIds.FirstOrDefault(x => x != senderUserId) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(recipientUserId))
            {
                return null;
            }

            var message = new ChatMessage
            {
                ConversationId = conversation.id ?? string.Empty,
                SenderUserId = senderUserId,
                RecipientUserId = recipientUserId,
                Content = model.Content,
                Attachments = model.Attachments,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            await _unitOfWork.Messages.InsertAsync(message);

            conversation.UpdatedAt = message.SentAt;
            conversation.LastMessageAt = message.SentAt;
            conversation.LastMessagePreview = BuildPreview(message.Content, message.Attachments);
            await _unitOfWork.Conversations.ReplaceAsync(conversation.id!, conversation);

            await _unitOfWork.SaveChangesAsync();

            await _notificationService.CreateAsync(
                recipientUserId,
                senderUserId,
                NotificationType.Message,
                "New message",
                message.id ?? string.Empty,
                new Dictionary<string, string>
                {
                    { "conversationId", conversation.id ?? string.Empty }
                });

            return message;
        }

        public async Task<List<MessageRequest>> GetPendingRequestsAsync(string userId)
        {
            return await _unitOfWork.MessageRequests.GetPendingForRecipientAsync(userId);
        }

        public async Task<List<Conversation>> GetConversationsAsync(string userId)
        {
            return await _unitOfWork.Conversations.GetForUserAsync(userId);
        }

        public async Task<List<ChatMessage>> GetConversationMessagesAsync(string userId, string conversationId, int skip, int take)
        {
            var canAccessConversation = await CanAccessConversationAsync(userId, conversationId);
            if (!canAccessConversation)
            {
                return new List<ChatMessage>();
            }

            var normalizedTake = Math.Clamp(take, 1, 200);
            var normalizedSkip = Math.Max(skip, 0);
            var messages = await _unitOfWork.Messages.GetForConversationAsync(conversationId, normalizedSkip, normalizedTake);
            return messages.OrderBy(x => x.SentAt).ToList();
        }

        public async Task<bool> CanAccessConversationAsync(string userId, string conversationId)
        {
            var conversation = await _unitOfWork.Conversations.GetByIdAsync(conversationId);
            return conversation != null && conversation.ParticipantUserIds.Contains(userId);
        }

        private static bool IsValidAttachment(MediaAttachment attachment)
        {
            if (attachment == null || string.IsNullOrWhiteSpace(attachment.Url))
            {
                return false;
            }

            return Uri.TryCreate(attachment.Url, UriKind.Absolute, out _);
        }

        private static string BuildPreview(string content, List<MediaAttachment> attachments)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                return content.Length > 80 ? content[..80] : content;
            }

            if (attachments.Count == 0)
            {
                return string.Empty;
            }

            return attachments[0].Type switch
            {
                MediaAttachmentType.Photo => "Photo",
                MediaAttachmentType.Video => "Video",
                MediaAttachmentType.Link => "Link",
                _ => "Attachment"
            };
        }
    }
}
