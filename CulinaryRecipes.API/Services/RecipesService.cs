using CulinaryRecipes.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Services
{
    public class RecipesService
    {
        private readonly IMongoCollection<Recipes> _recipesCollection;

        public RecipesService(IOptions<CulinaryRecipesDatabaseSettings> options)
        {
            var mongoClient = new MongoClient(options.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(options.Value.DatabaseName);


            _recipesCollection = mongoDatabase.GetCollection<Recipes>(
            options.Value.RecipesCollectionName);
        }

        public async Task<List<Recipes>> GetAsync() =>
                await _recipesCollection.Find(_ => true).ToListAsync();

        public async Task<Recipes?> GetAsync(string id) =>
            await _recipesCollection.Find(x => x.id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Recipes newRecipes) =>
            await _recipesCollection.InsertOneAsync(newRecipes);

        public async Task UpdateAsync(string id, Recipes updatedRecipes) =>
            await _recipesCollection.ReplaceOneAsync(x => x.id == id, updatedRecipes);

        public async Task RemoveAsync(string id) =>
            await _recipesCollection.DeleteOneAsync(x => x.id == id);

        public List<string> GetCategories(string searchText)
        {
            var categories = new List<string>();
            if(!string.IsNullOrEmpty(searchText))
            {
                categories = _recipesCollection.AsQueryable().Where(x => x.category == searchText).Select(x => x.category).Distinct().ToList();
            }
            else
            {
                categories = _recipesCollection.AsQueryable().Select(x => x.category).Distinct().ToList();
            }
            return categories;
       
        }

    }
}
