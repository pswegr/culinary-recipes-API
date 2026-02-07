using CulinaryRecipes.API.Data.Messaging.Interfaces;
using CulinaryRecipes.API.Models.Messaging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Data.Messaging
{
    public class MessagingMongoDbContext : IMessagingMongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IMessagingCollectionNameResolver _collectionNameResolver;

        public MessagingMongoDbContext(
            IOptions<MessagingDatabaseSettings> settings,
            IMessagingCollectionNameResolver collectionNameResolver)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
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
