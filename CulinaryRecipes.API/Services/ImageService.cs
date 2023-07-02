using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Services.Interfaces;

namespace CulinaryRecipes.API.Services
{
    public class ImageService : IImageService
    {
        public async Task<Image> UploadImageAsync(UploadImage image)
        {
            return await Task.FromResult(
                new Image
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Image ",
                    Extension = ".png",
                    Url = "http://localhost:5000"
                });
        }

        public async Task<IEnumerable<Image>> GetImagesAsync()
        {
            return await Task.FromResult(Enumerable.Range(1, 11)
                .Select(index => new Image
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Image " + index,
                    Extension = ".png",
                    Url = "http://localhost:5000"
                }));
        }
    }
}
