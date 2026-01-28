using CulinaryRecipes.API.Data.Interfaces;
using CulinaryRecipes.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Data
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollectionNameResolver _collectionNameResolver;

        public MongoDbContext(
            IMongoClient mongoClient,
            IOptions<CulinaryRecipesDatabaseSettings> settings,
            IMongoCollectionNameResolver collectionNameResolver)
        {
            _database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _collectionNameResolver = collectionNameResolver;
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            var collectionName = _collectionNameResolver.GetCollectionName<T>();
            return _database.GetCollection<T>(collectionName);
        }
    }
}
