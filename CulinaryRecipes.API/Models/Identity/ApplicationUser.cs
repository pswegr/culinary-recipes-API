using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace CulinaryRecipes.API.Models.Identity
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoUser
    {
        public string Nick { get; set; } = string.Empty;
    }
}
