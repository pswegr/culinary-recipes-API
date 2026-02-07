namespace CulinaryRecipes.API.Models.Messaging
{
    public class MessagingDatabaseSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ConversationsCollectionName { get; set; } = string.Empty;
        public string MessagesCollectionName { get; set; } = string.Empty;
        public string MessageRequestsCollectionName { get; set; } = string.Empty;
        public string NotificationsCollectionName { get; set; } = string.Empty;
    }
}
