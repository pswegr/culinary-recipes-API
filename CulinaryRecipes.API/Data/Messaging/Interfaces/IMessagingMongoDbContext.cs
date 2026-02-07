using MongoDB.Driver;

namespace CulinaryRecipes.API.Data.Messaging.Interfaces
{
    public interface IMessagingMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>();
    }
}
