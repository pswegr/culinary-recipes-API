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
        private readonly IMongoCollection<Recipe> _recipesCollection;

        public RecipesService(IOptions<CulinaryRecipesDatabaseSettings> options)
        {
            var mongoClient = new MongoClient(options.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(options.Value.DatabaseName);


            _recipesCollection = mongoDatabase.GetCollection<Recipe>(
            options.Value.RecipesCollectionName);
        }

        public async Task<List<Recipe>> GetAsync(string[]? tags = null, string? category = null, bool? publishedOnly = false, string? userNick = "", string? content = "")
        {
            var filterList = new List<FilterDefinition<Recipe>>();

            if(tags != null && tags.Length > 0)
            {
                filterList.Add(Builders<Recipe>.Filter.All(x => x.tags, tags));
            }

            if (!string.IsNullOrEmpty(category))
            {
                filterList.Add(Builders<Recipe>.Filter.Eq(x => x.category, category));
            }

            if(publishedOnly ?? false)
            {
                filterList.Add(Builders<Recipe>.Filter.Eq(x => x.published, true));
            }

            if (!string.IsNullOrEmpty(userNick)) 
            {
                filterList.Add(Builders<Recipe>.Filter.Eq(x => x.createdBy, userNick));
            }

            if (!string.IsNullOrEmpty(content))
            {
                var regex = new BsonRegularExpression(content, "i");
                filterList.Add(Builders<Recipe>.Filter.Regex("title", regex) |
                          Builders<Recipe>.Filter.Regex("description", regex) |
                          Builders<Recipe>.Filter.Regex("createdBy", regex) |
                          Builders<Recipe>.Filter.Regex("category", regex) |
                          Builders<Recipe>.Filter.Regex("instructions", regex) |
                          Builders<Recipe>.Filter.Regex("tags", regex) |                         
                          Builders<Recipe>.Filter.ElemMatch(r => r.ingredients, Builders<Ingredient>.Filter.Regex("name", regex)) |
                          Builders<Recipe>.Filter.ElemMatch(r => r.ingredients, Builders<Ingredient>.Filter.Regex("quantity", regex)));
            }

            filterList.Add(Builders<Recipe>.Filter.Eq(x => x.isActive, true));


            var filter = Builders<Recipe>.Filter.And(filterList);

            return await _recipesCollection.Find(filter).ToListAsync();
        }

        public async Task<Recipe?> GetAsync(string id) =>
            await _recipesCollection.Find(x => x.id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Recipe newRecipes, ImageUploadResult imageUploadResult, string userNick) 
        {
            newRecipes.createdAt = DateTime.UtcNow;
            newRecipes.createdBy = userNick;
            newRecipes.isActive = true;

            if (imageUploadResult?.SecureUrl?.AbsoluteUri != null)
            {
                newRecipes.photo = new Photo
                {
                    url = imageUploadResult.SecureUrl.AbsoluteUri,
                    publicId = imageUploadResult.PublicId,
                    mainColor = imageUploadResult.Colors[0][0]
                };
            }
            else
            {
                newRecipes.photo = new Photo
                {
                    url = "",
                    publicId = "",
                    mainColor = ""
                };
            }

            await _recipesCollection.InsertOneAsync(newRecipes); 
        }

        public async Task UpdateAsync(string id, Recipe updatedRecipes, ImageUploadResult imageUploadResult, string nick)
        {
            updatedRecipes.updatedAt = DateTime.UtcNow;
            updatedRecipes.updatedBy = nick;
            updatedRecipes.isActive = true;

            if (imageUploadResult?.SecureUrl?.AbsoluteUri != null)
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

        public async Task RemoveAsync(string id, Recipe recipe )
        {
            recipe.isActive = false;
            await _recipesCollection.ReplaceOneAsync(x => x.id == id, recipe);
        }

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

        public async Task<List<string>> GetTags(bool? publishedOnly = false, string? userNick = "")
        {
            var filterList = new List<FilterDefinition<Recipe>>();

            if(publishedOnly ?? false)
            {
                filterList.Add(Builders<Recipe>.Filter.Eq(x => x.published, true));
            }

            if (!string.IsNullOrEmpty(userNick)){
                filterList.Add(Builders<Recipe>.Filter.Eq(x => x.createdBy, userNick));
            }
        
            filterList.Add(Builders<Recipe>.Filter.Eq(x => x.isActive, true));

            var filter = Builders<Recipe>.Filter.And(filterList);
            if (filter == null)
            {
                return new List<string>();
            }

            var tagList = await _recipesCollection.Distinct<string>("tags", filter).ToListAsync();
            return tagList;
        }

        public async Task<List<string>> GetFavoritesTags(string? userId, bool? publishedOnly = false)
        {
            var filterList = new List<FilterDefinition<Recipe>>();

            if (publishedOnly ?? false)
            {
                filterList.Add(Builders<Recipe>.Filter.Eq(x => x.published, true));
            }

            if (!string.IsNullOrEmpty(userId))
            {
                var regex = new BsonRegularExpression(userId, "i");
                filterList.Add(Builders<Recipe>.Filter.Regex(x => x.LikedByUsers, regex));
            }

            filterList.Add(Builders<Recipe>.Filter.Eq(x => x.isActive, true));

            var filter = Builders<Recipe>.Filter.And(filterList);
            if (filter == null)
            {
                return new List<string>();
            }

            var tagList = await _recipesCollection.Distinct<string>("tags", filter).ToListAsync();
            return tagList;
        }

        public async Task LikeRecipeToggleAsync(string recipeId, string userId)
        {
            var recipe = await _recipesCollection.FindAsync(x => x.id == recipeId);

            if(recipe == null)
            {
                return;
            }

            if (recipe.FirstOrDefault().LikedByUsers.Contains(userId))
            {
                var filter = Builders<Recipe>.Filter.Eq(r => r.id, recipeId);
                var update = Builders<Recipe>.Update.Pull(r => r.LikedByUsers, userId);
                await _recipesCollection.UpdateOneAsync(filter, update);
            }
            else
            {
                var filter = Builders<Recipe>.Filter.Eq(r => r.id, recipeId);
                var update = Builders<Recipe>.Update.AddToSet(r => r.LikedByUsers, userId);
                await _recipesCollection.UpdateOneAsync(filter, update);
            }
        }

        public async Task<List<Recipe>> GetFavoritesAsync(string userId, string[]? tags = null, string? category = null, bool? publishedOnly = null, string? content = "")
        {
            var filterList = new List<FilterDefinition<Recipe>>();

            if (!string.IsNullOrEmpty(userId))
            {
                var regex = new BsonRegularExpression(userId, "i");
                filterList.Add(Builders<Recipe>.Filter.Regex(x => x.LikedByUsers, regex));
            }

            if (tags != null && tags.Length > 0)
            {
                filterList.Add(Builders<Recipe>.Filter.All(x => x.tags, tags));
            }

            if (!string.IsNullOrEmpty(category))
            {
                filterList.Add(Builders<Recipe>.Filter.Eq(x => x.category, category));
            }

            if (publishedOnly ?? false)
            {
                filterList.Add(Builders<Recipe>.Filter.Eq(x => x.published, true));
            }


            if (!string.IsNullOrEmpty(content))
            {
                var regex = new BsonRegularExpression(content, "i");
                filterList.Add(Builders<Recipe>.Filter.Regex("title", regex) |
                          Builders<Recipe>.Filter.Regex("description", regex) |
                          Builders<Recipe>.Filter.Regex("createdBy", regex) |
                          Builders<Recipe>.Filter.Regex("category", regex) |
                          Builders<Recipe>.Filter.Regex("instructions", regex) |
                          Builders<Recipe>.Filter.Regex("tags", regex) |
                          Builders<Recipe>.Filter.ElemMatch(r => r.ingredients, Builders<Ingredient>.Filter.Regex("name", regex)) |
                          Builders<Recipe>.Filter.ElemMatch(r => r.ingredients, Builders<Ingredient>.Filter.Regex("quantity", regex)));
            }

            filterList.Add(Builders<Recipe>.Filter.Eq(x => x.isActive, true));


            var filter = Builders<Recipe>.Filter.And(filterList);

            return await _recipesCollection.Find(filter).ToListAsync();
        }
    }
}
