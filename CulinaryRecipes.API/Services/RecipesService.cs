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

        public async Task<List<Recipes>> GetAsync(string[]? tags = null, string? category = null, bool? publishedOnly = false, string? userNick = "", string? content = "")
        {
            var filterList = new List<FilterDefinition<Recipes>>();

            if(tags != null && tags.Length > 0)
            {
                filterList.Add(Builders<Recipes>.Filter.All(x => x.tags, tags));
            }

            if (!string.IsNullOrEmpty(category))
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.category, category));
            }

            if(publishedOnly ?? false)
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.published, true));
            }

            if (!string.IsNullOrEmpty(userNick)) 
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.createdBy, userNick));
            }

            if (!string.IsNullOrEmpty(content))
            {
                var regex = new BsonRegularExpression(content, "i");
                filterList.Add(Builders<Recipes>.Filter.Regex("title", regex) |
                          Builders<Recipes>.Filter.Regex("description", regex) |
                          Builders<Recipes>.Filter.Regex("createdBy", regex) |
                          Builders<Recipes>.Filter.Regex("category", regex) |
                          Builders<Recipes>.Filter.Regex("instructions", regex) |
                          Builders<Recipes>.Filter.Regex("tags", regex) |                         
                          Builders<Recipes>.Filter.ElemMatch(r => r.ingredients, Builders<Ingredient>.Filter.Regex("name", regex)) |
                          Builders<Recipes>.Filter.ElemMatch(r => r.ingredients, Builders<Ingredient>.Filter.Regex("quantity", regex)));
            }

            filterList.Add(Builders<Recipes>.Filter.Eq(x => x.isActive, true));


            var filter = Builders<Recipes>.Filter.And(filterList);

            return await _recipesCollection.Find(filter).ToListAsync();
        }

        public async Task<Recipes?> GetAsync(string id) =>
            await _recipesCollection.Find(x => x.id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Recipes newRecipes, ImageUploadResult imageUploadResult, string userNick) 
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

        public async Task UpdateAsync(string id, Recipes updatedRecipes, ImageUploadResult imageUploadResult, string nick)
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

        public async Task RemoveAsync(string id, Recipes recipe )
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
            var filterList = new List<FilterDefinition<Recipes>>();

            if(publishedOnly ?? false)
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.published, true));
            }

            if (!string.IsNullOrEmpty(userNick)){
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.createdBy, userNick));
            }
        
            filterList.Add(Builders<Recipes>.Filter.Eq(x => x.isActive, true));

            var filter = Builders<Recipes>.Filter.And(filterList);
            if (filter == null)
            {
                return new List<string>();
            }

            var tagList = await _recipesCollection.Distinct<string>("tags", filter).ToListAsync();
            return tagList;
        }

        public async Task<List<string>> GetFavoritesTags(string? userId, bool? publishedOnly = false)
        {
            var filterList = new List<FilterDefinition<Recipes>>();

            if (publishedOnly ?? false)
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.published, true));
            }

            if (!string.IsNullOrEmpty(userId))
            {
                var regex = new BsonRegularExpression(userId, "i");
                filterList.Add(Builders<Recipes>.Filter.Regex(x => x.LikedByUsers, regex));
            }

            filterList.Add(Builders<Recipes>.Filter.Eq(x => x.isActive, true));

            var filter = Builders<Recipes>.Filter.And(filterList);
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
                var filter = Builders<Recipes>.Filter.Eq(r => r.id, recipeId);
                var update = Builders<Recipes>.Update.Pull(r => r.LikedByUsers, userId);
                await _recipesCollection.UpdateOneAsync(filter, update);
            }
            else
            {
                var filter = Builders<Recipes>.Filter.Eq(r => r.id, recipeId);
                var update = Builders<Recipes>.Update.AddToSet(r => r.LikedByUsers, userId);
                await _recipesCollection.UpdateOneAsync(filter, update);
            }
        }

        public async Task<List<Recipes>> GetFavoritesAsync(string userId, string[]? tags = null, string? category = null, bool? publishedOnly = null, string? content = "")
        {
            var filterList = new List<FilterDefinition<Recipes>>();

            if (!string.IsNullOrEmpty(userId))
            {
                var regex = new BsonRegularExpression(userId, "i");
                filterList.Add(Builders<Recipes>.Filter.Regex(x => x.LikedByUsers, regex));
            }

            if (tags != null && tags.Length > 0)
            {
                filterList.Add(Builders<Recipes>.Filter.All(x => x.tags, tags));
            }

            if (!string.IsNullOrEmpty(category))
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.category, category));
            }

            if (publishedOnly ?? false)
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.published, true));
            }


            if (!string.IsNullOrEmpty(content))
            {
                var regex = new BsonRegularExpression(content, "i");
                filterList.Add(Builders<Recipes>.Filter.Regex("title", regex) |
                          Builders<Recipes>.Filter.Regex("description", regex) |
                          Builders<Recipes>.Filter.Regex("createdBy", regex) |
                          Builders<Recipes>.Filter.Regex("category", regex) |
                          Builders<Recipes>.Filter.Regex("instructions", regex) |
                          Builders<Recipes>.Filter.Regex("tags", regex) |
                          Builders<Recipes>.Filter.ElemMatch(r => r.ingredients, Builders<Ingredient>.Filter.Regex("name", regex)) |
                          Builders<Recipes>.Filter.ElemMatch(r => r.ingredients, Builders<Ingredient>.Filter.Regex("quantity", regex)));
            }

            filterList.Add(Builders<Recipes>.Filter.Eq(x => x.isActive, true));


            var filter = Builders<Recipes>.Filter.And(filterList);

            return await _recipesCollection.Find(filter).ToListAsync();
        }
    }
}
