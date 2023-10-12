using CloudinaryDotNet.Actions;
using CulinaryRecipes.API.Models;

namespace CulinaryRecipes.API.Services.Interfaces
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> UploadPhotoAsync(IFormFile file);
    }
}
