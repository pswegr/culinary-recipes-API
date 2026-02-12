using CulinaryRecipes.API.Extensions.Claims;
using CulinaryRecipes.API.Models.Messaging;
using CulinaryRecipes.API.Models.Messaging.Requests;
using CulinaryRecipes.API.Services.Messaging.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryRecipes.API.Controllers.Messaging
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagingController : ControllerBase
    {
        private readonly IMessagingService _messagingService;

        public MessagingController(IMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        [HttpGet("conversations")]
        public async Task<ActionResult<List<Conversation>>> GetConversations()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var conversations = await _messagingService.GetConversationsAsync(userId);
            return Ok(conversations);
        }

        [HttpGet("conversations/{conversationId:length(24)}/messages")]
        public async Task<ActionResult<List<ChatMessage>>> GetMessages(string conversationId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var messages = await _messagingService.GetConversationMessagesAsync(userId, conversationId, skip, take);
            return Ok(messages);
        }

        [HttpGet("requests/pending")]
        public async Task<ActionResult<List<MessageRequest>>> GetPendingRequests()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var requests = await _messagingService.GetPendingRequestsAsync(userId);
            return Ok(requests);
        }

        [HttpPost("requests")]
        public async Task<ActionResult<MessageRequest>> CreateMessageRequest([FromBody] CreateMessageRequestModel model)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var recipientIdentifier = !string.IsNullOrWhiteSpace(model.RecipientNick)
                ? model.RecipientNick
                : model.RecipientUserId;

            var request = await _messagingService.CreateMessageRequestAsync(userId, recipientIdentifier);
            if (request == null)
            {
                return BadRequest("Message request cannot be created. Verify recipientNick (or recipientUserId), and ensure no existing conversation/request already exists.");
            }

            return Ok(request);
        }

        [HttpPost("requests/{requestId:length(24)}/respond")]
        public async Task<ActionResult<MessageRequest>> RespondMessageRequest(string requestId, [FromBody] RespondMessageRequestModel model)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var request = await _messagingService.RespondToMessageRequestAsync(requestId, userId, model.Accept);
            if (request == null)
            {
                return NotFound();
            }

            return Ok(request);
        }

        [HttpPost("messages")]
        public async Task<ActionResult<ChatMessage>> SendMessage([FromBody] SendMessageModel model)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var message = await _messagingService.SendMessageAsync(userId, model);
            if (message == null)
            {
                return BadRequest("Message cannot be sent.");
            }

            return Ok(message);
        }

        private bool TryGetUserId(out string userId)
        {
            userId = User.GetUserId() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            return true;
        }
    }
}
