using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CulinaryRecipes.API.Helpers;
using CulinaryRecipes.API.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CulinaryRecipes.API.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }
        public async Task<ImageUploadResult> UploadPhotoAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Height(563).Width(1000).Crop("fill").Gravity("face"),
                    Folder = "RecipesWithPassion",
                    Colors = true
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            return uploadResult;
        }
    }
}
