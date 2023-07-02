using CulinaryRecipes.API.Services.Interfaces;
using CulinaryRecipes.API.Services;

namespace CulinaryRecipes.API.Extensions
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IImageService, ImageService>();

            return services;
        }
    }
}
