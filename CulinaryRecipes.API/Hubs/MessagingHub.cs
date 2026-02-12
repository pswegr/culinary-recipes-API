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

            var request = await _messagingService.CreateMessageRequestAsync(senderUserId, recipientIdentifier);
            if (request == null)
            {
                throw new HubException("Message request cannot be created.");
            }

            await Clients.Caller.MessageRequestUpdated(request);
            await Clients.User(request.RecipientUserId).MessageRequestReceived(request);
        }

        public async Task RespondToMessageRequest(string requestId, RespondMessageRequestModel model)
        {
            var recipientUserId = GetRequiredUserId();
            var request = await _messagingService.RespondToMessageRequestAsync(requestId, recipientUserId, model.Accept);
            if (request == null)
            {
                throw new HubException("Message request cannot be updated.");
            }

            await Clients.Caller.MessageRequestUpdated(request);
            await Clients.User(request.SenderUserId).MessageRequestUpdated(request);
        }

        public async Task SendMessage(SendMessageModel model)
        {
            var senderUserId = GetRequiredUserId();
            var message = await _messagingService.SendMessageAsync(senderUserId, model);
            if (message == null)
            {
                throw new HubException("Message cannot be sent.");
            }

            await Clients.Caller.MessageReceived(message);
            await Clients.User(message.RecipientUserId).MessageReceived(message);
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
