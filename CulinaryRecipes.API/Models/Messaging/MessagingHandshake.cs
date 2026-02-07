namespace CulinaryRecipes.API.Models.Messaging
{
    public class MessagingHandshake
    {
        public string UserId { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public DateTime ServerTimeUtc { get; set; }
        public int PendingRequestCount { get; set; }
        public int UnreadNotificationCount { get; set; }
    }
}
