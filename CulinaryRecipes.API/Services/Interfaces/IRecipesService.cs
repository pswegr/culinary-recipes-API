using CloudinaryDotNet.Actions;
using CulinaryRecipes.API.Models;

namespace CulinaryRecipes.API.Services.Interfaces
{
    public interface IRecipesService
    {
        Task<List<Recipes>> GetAsync(string[]? tags = null, string? category = null, bool? publishedOnly = null, string? userNick = "", string? content = "");

        Task<Recipes?> GetAsync(string id);

        Task CreateAsync(Recipes newRecipes, ImageUploadResult imageUploadResult, string userNick);

        Task UpdateAsync(string id, Recipes updatedRecipes, ImageUploadResult imageUploadResult, string nick);

        Task RemoveAsync(string id, Recipes recipe);

        List<string> GetCategories(string searchText);

        Task<List<string>> GetTags(bool? publishedOnly = null, string? userNick = "");

        Task LikeRecipeToggleAsync(string recipeId, string userId);
    }
}
