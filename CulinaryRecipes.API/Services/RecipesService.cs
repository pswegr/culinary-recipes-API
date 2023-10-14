using CloudinaryDotNet.Actions;
using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Services
{
    public class RecipesService : IRecipesService
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

        public async Task<List<Recipes>> GetPublishedAsync() =>
               await _recipesCollection.Find(x => x.published).ToListAsync();

        public async Task<Recipes?> GetAsync(string id) =>
            await _recipesCollection.Find(x => x.id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Recipes newRecipes, ImageUploadResult imageUploadResult) 
        {
            newRecipes.createdAt = DateTime.UtcNow;
            newRecipes.createdBy = "TODO: Admin development";

            newRecipes.photo.url = imageUploadResult.SecureUrl.AbsoluteUri;
            newRecipes.photo.publicId = imageUploadResult.PublicId;
            newRecipes.photo.mainColor = imageUploadResult.Colors[0][0];
            await _recipesCollection.InsertOneAsync(newRecipes); 
        }

        public async Task UpdateAsync(string id, Recipes updatedRecipes, ImageUploadResult imageUploadResult)
        {
            updatedRecipes.updatedAt = DateTime.UtcNow;
            updatedRecipes.updatedBy = "TODO: Admin development";
            if (imageUploadResult.SecureUrl.AbsoluteUri != null)
            {
                updatedRecipes.photo = new Photo
                {
                    url = imageUploadResult.SecureUrl.AbsoluteUri,
                    publicId = imageUploadResult.PublicId,
                    mainColor = imageUploadResult.Colors[0][0]
                };
            }
        
            await _recipesCollection.ReplaceOneAsync(x => x.id == id, updatedRecipes);
        }

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

        public async Task<List<string>> GetTags()
        {
            var filter = new BsonDocument();
            var tagList = await _recipesCollection.Distinct<string>("tags", filter).ToListAsync();
            return tagList;
        }

    }
}
