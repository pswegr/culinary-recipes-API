using CulinaryRecipes.API.Extensions.Claims;
using CulinaryRecipes.API.Models.Messaging.Requests;
using CulinaryRecipes.API.Services.Messaging.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CulinaryRecipes.API.Hubs
{
    [Authorize]
    public class MessagingHub : Hub<IMessagingClient>
    {
        private readonly IMessagingService _messagingService;

        public MessagingHub(IMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                Context.Abort();
                return;
            }

            var handshake = await _messagingService.CreateHandshakeAsync(userId, Context.ConnectionId);
            await Clients.Caller.HandshakeAcknowledged(handshake);
            await base.OnConnectedAsync();
        }

        public async Task Handshake()
        {
            var userId = GetRequiredUserId();
            var handshake = await _messagingService.CreateHandshakeAsync(userId, Context.ConnectionId);
            await Clients.Caller.HandshakeAcknowledged(handshake);
        }

        public async Task SendMessageRequest(CreateMessageRequestModel model)
        {
            var senderUserId = GetRequiredUserId();
            var recipientIdentifier = !string.IsNullOrWhiteSpace(model.RecipientNick)
                ? model.RecipientNick
                : model.RecipientUserId;

            if (await _messagingService.CreateMessageRequestAsync(senderUserId, recipientIdentifier) == null)
            {
                throw new HubException("Message request cannot be created.");
            }
        }

        public async Task RespondToMessageRequest(string requestId, RespondMessageRequestModel model)
        {
            var recipientUserId = GetRequiredUserId();
            if (await _messagingService.RespondToMessageRequestAsync(requestId, recipientUserId, model.Accept) == null)
            {
                throw new HubException("Message request cannot be updated.");
            }
        }

        public async Task SendMessage(SendMessageModel model)
        {
            var senderUserId = GetRequiredUserId();
            if (await _messagingService.SendMessageAsync(senderUserId, model) == null)
            {
                throw new HubException("Message cannot be sent.");
            }
        }

        private string GetRequiredUserId()
        {
            var userId = Context.User?.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("User is not authenticated.");
            }

            return userId;
        }
    }
}
