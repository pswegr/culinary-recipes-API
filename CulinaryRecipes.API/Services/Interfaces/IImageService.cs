using CulinaryRecipes.API.Models;

namespace CulinaryRecipes.API.Services.Interfaces
{
    public interface IImageService
    {
        Task<Image> UploadImageAsync(UploadImage image);

        Task<IEnumerable<Image>> GetImagesAsync();
    }
}
