using CulinaryRecipes.API.Extensions.Claims;
using CulinaryRecipes.API.Models.Messaging;
using CulinaryRecipes.API.Services.Messaging.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryRecipes.API.Controllers.Messaging
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Notification>>> Get([FromQuery] bool unreadOnly = false, [FromQuery] int take = 50)
        {
            var userId = GetRequiredUserId();
            var notifications = await _notificationService.GetForUserAsync(userId, unreadOnly, Math.Clamp(take, 1, 200));
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<long>> GetUnreadCount()
        {
            var userId = GetRequiredUserId();
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(unreadCount);
        }

        [HttpPost("{notificationId:length(24)}/read")]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            var userId = GetRequiredUserId();
            var marked = await _notificationService.MarkAsReadAsync(userId, notificationId);
            if (!marked)
            {
                return NotFound();
            }

            return NoContent();
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
