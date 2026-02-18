using CulinaryRecipes.API.Hubs;
using CulinaryRecipes.API.Models.Identity;
using CulinaryRecipes.API.Models.Messaging;
using CulinaryRecipes.API.Models.Messaging.Requests;
using CulinaryRecipes.API.Services.Messaging.Interfaces;
using CulinaryRecipes.API.UnitOfWork.Messaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;

namespace CulinaryRecipes.API.Services.Messaging
{
    public class MessagingService : IMessagingService
    {
        private readonly IMessagingUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<MessagingHub, IMessagingClient> _hubContext;
        private readonly ILogger<MessagingService> _logger;

        public MessagingService(
            IMessagingUnitOfWork unitOfWork,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            IHubContext<MessagingHub, IMessagingClient> hubContext,
            ILogger<MessagingService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _userManager = userManager;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<MessagingHandshake> CreateHandshakeAsync(string userId, string connectionId)
        {
            List<MessageRequest> pendingRequests;
            try
            {
                pendingRequests = await _unitOfWork.MessageRequests.GetPendingForRecipientAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load pending message requests for user {UserId}.", userId);
                pendingRequests = new List<MessageRequest>();
            }

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
            if (string.IsNullOrWhiteSpace(senderUserId) || string.IsNullOrWhiteSpace(recipientUserId))
            {
                _logger.LogWarning("Cannot create message request because sender or recipient is empty.");
                return null;
            }

            var resolvedRecipientUserId = await ResolveRecipientUserIdAsync(recipientUserId);
            if (string.IsNullOrWhiteSpace(resolvedRecipientUserId))
            {
                _logger.LogWarning(
                    "Cannot create message request from {SenderUserId}: recipient '{RecipientInput}' could not be resolved.",
                    senderUserId,
                    recipientUserId);
                return null;
            }

            if (senderUserId == resolvedRecipientUserId)
            {
                _logger.LogWarning("Cannot create message request because sender {SenderUserId} equals recipient.", senderUserId);
                return null;
            }

            var existingConversation = await _unitOfWork.Conversations.GetByParticipantsAsync(senderUserId, resolvedRecipientUserId);
            if (existingConversation != null)
            {
                _logger.LogInformation(
                    "Message request skipped for users {SenderUserId}/{RecipientUserId} because a conversation already exists.",
                    senderUserId,
                    resolvedRecipientUserId);
                return null;
            }

            var existingRequest = await _unitOfWork.MessageRequests.GetPendingBetweenUsersAsync(senderUserId, resolvedRecipientUserId);
            if (existingRequest != null)
            {
                return existingRequest;
            }

            var request = new MessageRequest
            {
                SenderUserId = senderUserId,
                RecipientUserId = resolvedRecipientUserId,
                CreatedAt = DateTime.UtcNow,
                Status = MessageRequestStatus.Pending
            };

            await _unitOfWork.MessageRequests.InsertAsync(request);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.CreateAsync(
                resolvedRecipientUserId,
                senderUserId,
                NotificationType.MessageRequest,
                "New messaging request",
                request.id ?? string.Empty,
                new Dictionary<string, string>
                {
                    { "requestStatus", request.Status.ToString() }
                });

            await _hubContext.Clients.User(senderUserId).MessageRequestUpdated(request);
            await _hubContext.Clients.User(request.RecipientUserId).MessageRequestReceived(request);

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
            Conversation? conversationToBroadcast = null;
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
                        LastMessageAt = null,
                        LastMessageSenderUserId = string.Empty
                    };

                    await _unitOfWork.Conversations.InsertAsync(conversation);
                    conversationId = conversation.id ?? string.Empty;
                    conversationToBroadcast = conversation;
                }
                else
                {
                    conversationId = existingConversation.id ?? string.Empty;
                    conversationToBroadcast = existingConversation;
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

            await _hubContext.Clients.User(request.RecipientUserId).MessageRequestUpdated(request);
            await _hubContext.Clients.User(request.SenderUserId).MessageRequestUpdated(request);

            if (accept && conversationToBroadcast != null)
            {
                await PopulateConversationNickDataAsync(new List<Conversation> { conversationToBroadcast });
                await _hubContext.Clients.User(request.SenderUserId).ConversationUpdated(conversationToBroadcast);
                await _hubContext.Clients.User(request.RecipientUserId).ConversationUpdated(conversationToBroadcast);
            }

            return request;
        }

        public async Task<ChatMessage?> SendMessageAsync(string senderUserId, SendMessageModel model)
        {
            if (string.IsNullOrWhiteSpace(senderUserId) || model == null)
            {
                return null;
            }

            var attachments = model.Attachments ?? new List<MediaAttachment>();
            if (string.IsNullOrWhiteSpace(model.Content) && attachments.Count == 0)
            {
                return null;
            }

            if (attachments.Any(a => !IsValidAttachment(a)))
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

            var defaultRecipientUserId = conversation.ParticipantUserIds.FirstOrDefault(x => x != senderUserId) ?? string.Empty;
            var recipientUserId = !string.IsNullOrWhiteSpace(model.RecipientUserId)
                ? model.RecipientUserId
                : defaultRecipientUserId;

            if (!conversation.ParticipantUserIds.Contains(recipientUserId))
            {
                recipientUserId = defaultRecipientUserId;
            }

            if (string.IsNullOrWhiteSpace(recipientUserId) || recipientUserId == senderUserId)
            {
                return null;
            }

            var message = new ChatMessage
            {
                ConversationId = conversation.id ?? string.Empty,
                SenderUserId = senderUserId,
                RecipientUserId = recipientUserId,
                Content = model.Content,
                Attachments = attachments,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            await _unitOfWork.Messages.InsertAsync(message);

            var messagePreview = BuildPreview(message.Content, message.Attachments);
            conversation.UpdatedAt = message.SentAt;
            conversation.LastMessageAt = message.SentAt;
            conversation.LastMessagePreview = messagePreview;
            conversation.LastMessageSenderUserId = senderUserId;
            await _unitOfWork.Conversations.ReplaceAsync(conversation.id!, conversation);

            await _unitOfWork.SaveChangesAsync();

            await PopulateMessageNickDataAsync(new List<ChatMessage> { message });
            await PopulateConversationNickDataAsync(new List<Conversation> { conversation });

            await _notificationService.CreateAsync(
                recipientUserId,
                senderUserId,
                NotificationType.Message,
                "New message",
                message.id ?? string.Empty,
                new Dictionary<string, string>
                {
                    { "conversationId", conversation.id ?? string.Empty },
                    { "senderUserId", senderUserId },
                    { "senderNick", message.SenderNick },
                    { "messagePreview", messagePreview },
                    { "sentAtUtc", message.SentAt.ToString("O") }
                });

            var targetUserIds = new[] { senderUserId, recipientUserId }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            if (targetUserIds.Count > 0)
            {
                await _hubContext.Clients.Users(targetUserIds).MessageReceived(message);
                await _hubContext.Clients.Users(targetUserIds).ConversationUpdated(conversation);
            }

            await _hubContext.Clients.User(recipientUserId).MessageAlertReceived(new MessageAlert
            {
                ConversationId = conversation.id ?? string.Empty,
                MessageId = message.id ?? string.Empty,
                SenderUserId = senderUserId,
                SenderNick = message.SenderNick,
                Preview = messagePreview,
                SentAt = message.SentAt
            });

            return message;
        }

        public async Task<List<MessageRequest>> GetPendingRequestsAsync(string userId)
        {
            return await _unitOfWork.MessageRequests.GetPendingForRecipientAsync(userId);
        }

        public async Task<PagedResult<Conversation>> GetConversationsAsync(string userId, int skip, int take)
        {
            var normalizedTake = Math.Clamp(take, 1, 100);
            var normalizedSkip = Math.Max(skip, 0);
            var (conversations, totalCount) = await _unitOfWork.Conversations.GetForUserAsync(userId, normalizedSkip, normalizedTake);

            await PopulateConversationNickDataAsync(conversations);

            return new PagedResult<Conversation>
            {
                Items = conversations,
                Skip = normalizedSkip,
                Take = normalizedTake,
                TotalCount = totalCount
            };
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
            var orderedMessages = messages.OrderBy(x => x.SentAt).ToList();
            await PopulateMessageNickDataAsync(orderedMessages);

            return orderedMessages;
        }

        public async Task<bool> CanAccessConversationAsync(string userId, string conversationId)
        {
            var conversation = await _unitOfWork.Conversations.GetByIdAsync(conversationId);
            return conversation != null && conversation.ParticipantUserIds.Contains(userId);
        }

        private async Task PopulateConversationNickDataAsync(List<Conversation> conversations)
        {
            if (conversations.Count == 0)
            {
                return;
            }

            var userIds = conversations
                .SelectMany(c => c.ParticipantUserIds)
                .Concat(conversations.Select(c => c.LastMessageSenderUserId))
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var nickLookup = await GetNickLookupAsync(userIds);

            foreach (var conversation in conversations)
            {
                conversation.ParticipantNicks = conversation.ParticipantUserIds
                    .Distinct()
                    .ToDictionary(
                        userId => userId,
                        userId => nickLookup.TryGetValue(userId, out var nick) ? nick : userId);

                conversation.LastMessageSenderNick = string.IsNullOrWhiteSpace(conversation.LastMessageSenderUserId)
                    ? string.Empty
                    : nickLookup.TryGetValue(conversation.LastMessageSenderUserId, out var senderNick)
                        ? senderNick
                        : conversation.LastMessageSenderUserId;
            }
        }

        private async Task PopulateMessageNickDataAsync(List<ChatMessage> messages)
        {
            if (messages.Count == 0)
            {
                return;
            }

            var userIds = messages
                .SelectMany(m => new[] { m.SenderUserId, m.RecipientUserId })
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var nickLookup = await GetNickLookupAsync(userIds);

            foreach (var message in messages)
            {
                message.SenderNick = nickLookup.TryGetValue(message.SenderUserId, out var senderNick)
                    ? senderNick
                    : message.SenderUserId;

                message.RecipientNick = nickLookup.TryGetValue(message.RecipientUserId, out var recipientNick)
                    ? recipientNick
                    : message.RecipientUserId;
            }
        }

        private async Task<Dictionary<string, string>> GetNickLookupAsync(IEnumerable<string> userIds)
        {
            var distinctUserIds = userIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (distinctUserIds.Count == 0)
            {
                return new Dictionary<string, string>();
            }

            var lookupEntries = await Task.WhenAll(distinctUserIds.Select(async userId =>
            {
                var user = await _userManager.FindByIdAsync(userId);
                return new KeyValuePair<string, string>(userId, ResolveUserNick(user, userId));
            }));

            return lookupEntries.ToDictionary(x => x.Key, x => x.Value);
        }

        private static string ResolveUserNick(ApplicationUser? user, string fallbackUserId)
        {
            if (user == null)
            {
                return fallbackUserId;
            }

            if (!string.IsNullOrWhiteSpace(user.Nick))
            {
                return user.Nick;
            }

            if (!string.IsNullOrWhiteSpace(user.UserName))
            {
                return user.UserName;
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                return user.Email;
            }

            return fallbackUserId;
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

        private async Task<string?> ResolveRecipientUserIdAsync(string recipientInput)
        {
            var normalizedInput = recipientInput?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedInput))
            {
                return null;
            }

            if (ObjectId.TryParse(normalizedInput, out _))
            {
                var userById = await _userManager.FindByIdAsync(normalizedInput);
                if (userById != null)
                {
                    return userById.Id.ToString();
                }
            }

            var userByEmail = await _userManager.FindByEmailAsync(normalizedInput);
            if (userByEmail != null)
            {
                return userByEmail.Id.ToString();
            }

            var userByName = await _userManager.FindByNameAsync(normalizedInput);
            if (userByName != null)
            {
                return userByName.Id.ToString();
            }

            var userByNick = _userManager.Users.FirstOrDefault(u => u.Nick == normalizedInput);
            return userByNick?.Id.ToString();
        }
    }
}
