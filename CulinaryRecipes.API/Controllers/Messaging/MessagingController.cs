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
            var userId = GetRequiredUserId();
            var conversations = await _messagingService.GetConversationsAsync(userId);
            return Ok(conversations);
        }

        [HttpGet("conversations/{conversationId:length(24)}/messages")]
        public async Task<ActionResult<List<ChatMessage>>> GetMessages(string conversationId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            var userId = GetRequiredUserId();
            var messages = await _messagingService.GetConversationMessagesAsync(userId, conversationId, skip, take);
            return Ok(messages);
        }

        [HttpGet("requests/pending")]
        public async Task<ActionResult<List<MessageRequest>>> GetPendingRequests()
        {
            var userId = GetRequiredUserId();
            var requests = await _messagingService.GetPendingRequestsAsync(userId);
            return Ok(requests);
        }

        [HttpPost("requests")]
        public async Task<ActionResult<MessageRequest>> CreateMessageRequest([FromBody] CreateMessageRequestModel model)
        {
            var userId = GetRequiredUserId();
            var request = await _messagingService.CreateMessageRequestAsync(userId, model.RecipientUserId);
            if (request == null)
            {
                return BadRequest("Message request cannot be created.");
            }

            return Ok(request);
        }

        [HttpPost("requests/{requestId:length(24)}/respond")]
        public async Task<ActionResult<MessageRequest>> RespondMessageRequest(string requestId, [FromBody] RespondMessageRequestModel model)
        {
            var userId = GetRequiredUserId();
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
            var userId = GetRequiredUserId();
            var message = await _messagingService.SendMessageAsync(userId, model);
            if (message == null)
            {
                return BadRequest("Message cannot be sent.");
            }

            return Ok(message);
        }

        private string GetRequiredUserId()
        {
            var userId = User.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return userId;
        }
    }
}
