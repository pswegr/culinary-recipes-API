using CulinaryRecipes.API.Models.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Data;

public class MongoDbIdentityContext
{
    private readonly IMongoDatabase _database;

    public MongoDbIdentityContext(IOptions<IdentityDatabaseSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }

}
