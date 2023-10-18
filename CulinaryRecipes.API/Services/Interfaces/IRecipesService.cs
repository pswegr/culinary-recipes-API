using CloudinaryDotNet.Actions;
using CulinaryRecipes.API.Models;
using MongoDB.Bson;

namespace CulinaryRecipes.API.Services.Interfaces
{
    public interface IRecipesService
    {
        Task<List<Recipes>> GetAsync();

        Task<List<Recipes>> GetPublishedAsync(string[]? tags, string? category);

        Task<Recipes?> GetAsync(string id);

        Task CreateAsync(Recipes newRecipes, ImageUploadResult imageUploadResult);

        Task UpdateAsync(string id, Recipes updatedRecipes, ImageUploadResult imageUploadResult);

        Task RemoveAsync(string id, Recipes recipe);

        List<string> GetCategories(string searchText);
        Task<List<string>> GetTags();
    }
}
