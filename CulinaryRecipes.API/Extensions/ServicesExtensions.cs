using CulinaryRecipes.API.Services.Interfaces;
using CulinaryRecipes.API.Services;
using CulinaryRecipes.API.Helpers;

namespace CulinaryRecipes.API.Extensions
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<IRecipesService, RecipesService>();
            services.AddScoped<IPhotoService, PhotoService>();

            return services;
        }
    }
}
