using CulinaryRecipes.API.Models.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CulinaryRecipes.API.Models.Messaging
{
    public class Notification : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }

        public string RecipientUserId { get; set; } = string.Empty;
        public string ActorUserId { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ReferenceId { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
