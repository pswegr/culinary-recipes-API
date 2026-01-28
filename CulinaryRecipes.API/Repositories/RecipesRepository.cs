using CulinaryRecipes.API.Data.Dao;
using CulinaryRecipes.API.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Repositories
{
    public class RecipesRepository : MongoGenericRepository<Recipes>, IRecipesRepository
    {
        private readonly IGenericDao<Recipes> _dao;

        public RecipesRepository(IGenericDao<Recipes> dao) : base(dao)
        {
            _dao = dao;
        }

        public async Task<List<Recipes>> GetFilteredAsync(string[]? tags = null, string? category = null, bool? publishedOnly = null, string? userNick = "", string? content = "")
        {
            var filter = BuildBaseFilter(tags, category, publishedOnly, userNick, content);
            return await _dao.Collection.Find(filter).ToListAsync();
        }

        public async Task<List<Recipes>> GetFavoritesAsync(string? userId, string[]? tags = null, string? category = null, bool? publishedOnly = null, string? content = "")
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
                filterList.Add(BuildContentFilter(content));
            }

            filterList.Add(Builders<Recipes>.Filter.Eq(x => x.isActive, true));

            var filter = Builders<Recipes>.Filter.And(filterList);
            return await _dao.Collection.Find(filter).ToListAsync();
        }

        public List<string> GetCategories(string? searchText)
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                return _dao.Collection.AsQueryable()
                    .Where(x => x.category == searchText)
                    .Select(x => x.category)
                    .Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            }

            return _dao.Collection.AsQueryable()
                .Select(x => x.category)
                .Distinct()
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
        }

        public async Task<List<string>> GetTagsAsync(bool? publishedOnly = null, string? userNick = "")
        {
            var filterList = new List<FilterDefinition<Recipes>>();

            if (publishedOnly ?? false)
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.published, true));
            }

            if (!string.IsNullOrEmpty(userNick))
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.createdBy, userNick));
            }

            filterList.Add(Builders<Recipes>.Filter.Eq(x => x.isActive, true));

            var filter = Builders<Recipes>.Filter.And(filterList);
            return await _dao.Collection.Distinct<string>("tags", filter).ToListAsync();
        }

        public async Task<List<string>> GetFavoritesTagsAsync(string? userId, bool? publishedOnly = null)
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
            return await _dao.Collection.Distinct<string>("tags", filter).ToListAsync();
        }

        public async Task<bool> LikeRecipeToggleAsync(string recipeId, string userId)
        {
            var recipe = await _dao.Collection.Find(x => x.id == recipeId).FirstOrDefaultAsync();
            if (recipe == null)
            {
                return false;
            }

            var filter = Builders<Recipes>.Filter.Eq(r => r.id, recipeId);
            UpdateDefinition<Recipes> update;

            if (recipe.LikedByUsers.Contains(userId))
            {
                update = Builders<Recipes>.Update.Pull(r => r.LikedByUsers, userId);
            }
            else
            {
                update = Builders<Recipes>.Update.AddToSet(r => r.LikedByUsers, userId);
            }

            await _dao.Collection.UpdateOneAsync(filter, update);
            return true;
        }

        private static FilterDefinition<Recipes> BuildBaseFilter(
            string[]? tags,
            string? category,
            bool? publishedOnly,
            string? userNick,
            string? content)
        {
            var filterList = new List<FilterDefinition<Recipes>>();

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

            if (!string.IsNullOrEmpty(userNick))
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.createdBy, userNick));
            }

            if (!string.IsNullOrEmpty(content))
            {
                filterList.Add(BuildContentFilter(content));
            }

            filterList.Add(Builders<Recipes>.Filter.Eq(x => x.isActive, true));

            return Builders<Recipes>.Filter.And(filterList);
        }

        private static FilterDefinition<Recipes> BuildContentFilter(string content)
        {
            var regex = new BsonRegularExpression(content, "i");
            return Builders<Recipes>.Filter.Regex("title", regex) |
                   Builders<Recipes>.Filter.Regex("description", regex) |
                   Builders<Recipes>.Filter.Regex("createdBy", regex) |
                   Builders<Recipes>.Filter.Regex("category", regex) |
                   Builders<Recipes>.Filter.Regex("instructions", regex) |
                   Builders<Recipes>.Filter.Regex("tags", regex) |
                   Builders<Recipes>.Filter.ElemMatch(r => r.ingredients, Builders<Ingredient>.Filter.Regex("name", regex)) |
                   Builders<Recipes>.Filter.ElemMatch(r => r.ingredients, Builders<Ingredient>.Filter.Regex("quantity", regex));
        }
    }
}
