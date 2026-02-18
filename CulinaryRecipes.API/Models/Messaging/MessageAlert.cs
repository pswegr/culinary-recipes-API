namespace CulinaryRecipes.API.Models.Messaging
{
    public class MessageAlert
    {
        public string ConversationId { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public string SenderUserId { get; set; } = string.Empty;
        public string SenderNick { get; set; } = string.Empty;
        public string Preview { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
