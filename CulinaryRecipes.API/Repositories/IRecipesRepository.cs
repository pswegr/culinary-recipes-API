using CulinaryRecipes.API.Models;

namespace CulinaryRecipes.API.Repositories
{
    public interface IRecipesRepository : IGenericRepository<Recipes>
    {
        Task<List<Recipes>> GetFilteredAsync(string[]? tags = null, string? category = null, bool? publishedOnly = null, string? userNick = "", string? content = "");
        Task<List<Recipes>> GetFavoritesAsync(string? userId, string[]? tags = null, string? category = null, bool? publishedOnly = null, string? content = "");
        List<string> GetCategories(string? searchText);
        Task<List<string>> GetTagsAsync(bool? publishedOnly = null, string? userNick = "");
        Task<List<string>> GetFavoritesTagsAsync(string? userId, bool? publishedOnly = null);
        Task<bool> LikeRecipeToggleAsync(string recipeId, string userId);
    }
}
