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
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var notifications = await _notificationService.GetForUserAsync(userId, unreadOnly, Math.Clamp(take, 1, 200));
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<long>> GetUnreadCount()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(unreadCount);
        }

        [HttpPost("{notificationId:length(24)}/read")]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var marked = await _notificationService.MarkAsReadAsync(userId, notificationId);
            if (!marked)
            {
                return NotFound();
            }

            return NoContent();
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
