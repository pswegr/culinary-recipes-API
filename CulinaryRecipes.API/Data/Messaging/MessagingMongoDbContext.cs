using CulinaryRecipes.API.Data.Messaging.Interfaces;
using CulinaryRecipes.API.Models;
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
            IMongoClient mongoClient,
            IOptions<MessagingDatabaseSettings> settings,
            IOptions<CulinaryRecipesDatabaseSettings> mainDbSettings,
            ILogger<MessagingMongoDbContext> logger,
            IMessagingCollectionNameResolver collectionNameResolver)
        {
            var messagingConnectionString = settings.Value.ConnectionString;
            var shouldFallbackToMainConnection =
                string.IsNullOrWhiteSpace(messagingConnectionString) ||
                IsLocalhostConnectionString(messagingConnectionString) &&
                !IsLocalhostConnectionString(mainDbSettings.Value.ConnectionString);
            var databaseName = string.IsNullOrWhiteSpace(settings.Value.DatabaseName)
                ? mainDbSettings.Value.DatabaseName
                : settings.Value.DatabaseName;

            var messagingClient = shouldFallbackToMainConnection
                ? mongoClient
                : new MongoClient(messagingConnectionString);

            if (shouldFallbackToMainConnection)
            {
                logger.LogWarning(
                    "Messaging database connection string is empty or local while primary MongoDB is configured. Falling back to primary Mongo client.");
            }

            _database = messagingClient.GetDatabase(databaseName);
            _collectionNameResolver = collectionNameResolver;
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            var collectionName = _collectionNameResolver.GetCollectionName<T>();
            return _database.GetCollection<T>(collectionName);
        }

        private static bool IsLocalhostConnectionString(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            return connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);
        }
    }
}
