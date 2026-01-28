using CulinaryRecipes.API.Data.Attributes;
using CulinaryRecipes.API.Data.Interfaces;
using CulinaryRecipes.API.Models;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace CulinaryRecipes.API.Data
{
    public class MongoCollectionNameResolver : IMongoCollectionNameResolver
    {
        private readonly IOptions<CulinaryRecipesDatabaseSettings> _settings;

        public MongoCollectionNameResolver(IOptions<CulinaryRecipesDatabaseSettings> settings)
        {
            _settings = settings;
        }

        public string GetCollectionName<T>()
        {
            if (typeof(T) == typeof(Recipes))
            {
                return _settings.Value.RecipesCollectionName;
            }

            var attribute = typeof(T).GetCustomAttribute<BsonCollectionAttribute>();
            if (attribute != null)
            {
                return attribute.CollectionName;
            }

            throw new InvalidOperationException($"No collection mapping found for type '{typeof(T).Name}'.");
        }
    }
}
