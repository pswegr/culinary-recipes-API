using CloudinaryDotNet.Actions;
using CulinaryRecipes.API.Models;
using MongoDB.Bson;

namespace CulinaryRecipes.API.Services.Interfaces
{
    public interface IRecipesService
    {
        Task<List<Recipes>> GetAsync();

        Task<Recipes?> GetAsync(string id);

        Task CreateAsync(Recipes newRecipes, ImageUploadResult imageUploadResult);

        Task UpdateAsync(string id, Recipes updatedRecipes, ImageUploadResult imageUploadResult);

        Task RemoveAsync(string id);

        List<string> GetCategories(string searchText);
        Task<List<string>> GetTags();
    }
}
