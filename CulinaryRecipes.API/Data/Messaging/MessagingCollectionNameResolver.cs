using CulinaryRecipes.API.Data.Messaging.Interfaces;
using CulinaryRecipes.API.Models.Messaging;
using Microsoft.Extensions.Options;

namespace CulinaryRecipes.API.Data.Messaging
{
    public class MessagingCollectionNameResolver : IMessagingCollectionNameResolver
    {
        private readonly IOptions<MessagingDatabaseSettings> _settings;

        public MessagingCollectionNameResolver(IOptions<MessagingDatabaseSettings> settings)
        {
            _settings = settings;
        }

        public string GetCollectionName<T>()
        {
            if (typeof(T) == typeof(Conversation))
            {
                return _settings.Value.ConversationsCollectionName;
            }

            if (typeof(T) == typeof(ChatMessage))
            {
                return _settings.Value.MessagesCollectionName;
            }

            if (typeof(T) == typeof(MessageRequest))
            {
                return _settings.Value.MessageRequestsCollectionName;
            }

            if (typeof(T) == typeof(Notification))
            {
                return _settings.Value.NotificationsCollectionName;
            }

            throw new InvalidOperationException($"No messaging collection mapping found for type '{typeof(T).Name}'.");
        }
    }
}
