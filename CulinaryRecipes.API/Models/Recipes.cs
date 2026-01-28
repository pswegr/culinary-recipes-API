using MongoDB.Bson.Serialization.Attributes;

using CulinaryRecipes.API.Models.Interfaces;

namespace CulinaryRecipes.API.Models
{
    public class Recipes : IEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? id { get; set; }
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int preparationTime { get; set; }
        public int cookingTime { get; set; }
        public int servings { get; set; }
        public string category { get; set; } = string.Empty;
        public string imageUrl { get; set; } = string.Empty;
        public List<Ingredient> ingredients { get; set; } = new();
        public List<string> instructions { get; set; } = new();
        public List<string> tags { get; set; } = new();
        public string? createdBy { get; set; } = string.Empty;
        public string? updatedBy { get; set; } = string.Empty;
        public DateTime? createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
        public Photo? photo { get; set; }
        public bool published { get; set; }
        public bool isActive {  get; set; }
        public List<string> LikedByUsers { get; set; } = new List<string>();
    }
}
