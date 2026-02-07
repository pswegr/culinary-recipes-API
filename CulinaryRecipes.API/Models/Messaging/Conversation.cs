using CulinaryRecipes.API.Models.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CulinaryRecipes.API.Models.Messaging
{
    public class Conversation : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }

        public List<string> ParticipantUserIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string LastMessagePreview { get; set; } = string.Empty;
        public DateTime? LastMessageAt { get; set; }
    }
}
