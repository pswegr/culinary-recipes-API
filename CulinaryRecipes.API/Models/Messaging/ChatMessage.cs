using CulinaryRecipes.API.Models.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CulinaryRecipes.API.Models.Messaging
{
    public class ChatMessage : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }

        public string ConversationId { get; set; } = string.Empty;
        public string SenderUserId { get; set; } = string.Empty;
        public string RecipientUserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<MediaAttachment> Attachments { get; set; } = new();
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }

        [BsonIgnore]
        public string SenderNick { get; set; } = string.Empty;

        [BsonIgnore]
        public string RecipientNick { get; set; } = string.Empty;
    }
}
