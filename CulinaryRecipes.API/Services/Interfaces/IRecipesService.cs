using CloudinaryDotNet.Actions;
using CulinaryRecipes.API.Models;

namespace CulinaryRecipes.API.Services.Interfaces
{
    public interface IRecipesService
    {
        Task<List<Recipe>> GetAsync(string[]? tags = null, string? category = null, bool? publishedOnly = null, string? userNick = "", string? content = "");

        Task<List<Recipe>> GetFavoritesAsync(string userId, string[]? tags = null, string? category = null, bool? publishedOnly = null, string? content = "");

        Task<Recipe?> GetAsync(string id);

        Task CreateAsync(Recipe newRecipes, ImageUploadResult imageUploadResult, string userNick);

        Task UpdateAsync(string id, Recipe updatedRecipes, ImageUploadResult imageUploadResult, string nick);

        Task RemoveAsync(string id, Recipe recipe);

        List<string> GetCategories(string searchText);

        Task<List<string>> GetTags(bool? publishedOnly = null, string? userNick = "");

        Task<List<string>> GetFavoritesTags(string userId, bool? publishedOnly = null);

        Task LikeRecipeToggleAsync(string recipeId, string userId);
    }
}
