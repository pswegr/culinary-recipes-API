using MongoDB.Bson.Serialization.Attributes;

namespace CulinaryRecipes.API.Models
{
    public class Recipes
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int preparationTime { get; set; }
        public int cookingTime { get; set; }
        public int servings { get; set; }
        public string category { get; set; }
        public string imageUrl { get; set; }
        public List<Ingredient> ingredients { get; set; }
        public List<string> instructions { get; set; }
        public List<string> tags { get; set; }
        public string? createdBy { get; set; } = string.Empty;
        public string? updatedBy { get; set; } = string.Empty;
        public DateTime? createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
        public Photo? photo { get; set; }
    
    }
}
