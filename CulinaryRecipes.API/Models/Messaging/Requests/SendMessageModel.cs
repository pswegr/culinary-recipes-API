namespace CulinaryRecipes.API.Models.Messaging.Requests
{
    public class SendMessageModel
    {
        public string ConversationId { get; set; } = string.Empty;
        public string RecipientUserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<MediaAttachment> Attachments { get; set; } = new();
    }
}
