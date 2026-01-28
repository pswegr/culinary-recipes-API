using MongoDB.Driver;

namespace CulinaryRecipes.API.Data.Interfaces
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>();
    }
}
