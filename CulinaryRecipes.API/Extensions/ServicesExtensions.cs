using CulinaryRecipes.API.Data;
using CulinaryRecipes.API.Data.Dao;
using CulinaryRecipes.API.Data.Interfaces;
using CulinaryRecipes.API.Helpers;
using CulinaryRecipes.API.Mediation;
using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Repositories;
using CulinaryRecipes.API.Services;
using CulinaryRecipes.API.Services.Interfaces;
using CulinaryRecipes.API.UnitOfWork;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Reflection;

namespace CulinaryRecipes.API.Extensions
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<CulinaryRecipesDatabaseSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });
            services.AddSingleton<IMongoCollectionNameResolver, MongoCollectionNameResolver>();
            services.AddScoped<IMongoDbContext, MongoDbContext>();
            services.AddScoped(typeof(IGenericDao<>), typeof(MongoGenericDao<>));
            services.AddScoped(typeof(IGenericRepository<>), typeof(MongoGenericRepository<>));
            services.AddScoped<IRecipesRepository, RecipesRepository>();
            services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
            services.AddMediation();

            return services;
        }

        public static IServiceCollection AddMediation(this IServiceCollection services)
        {
            services.AddScoped<IMediator, Mediator>();

            var handlerInterface = typeof(IRequestHandler<,>);
            var handlers = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .SelectMany(type => type.GetInterfaces()
                    .Where(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == handlerInterface)
                    .Select(@interface => new { Handler = type, Interface = @interface }));

            foreach (var handler in handlers)
            {
                services.AddScoped(handler.Interface, handler.Handler);
            }

            return services;
        }
    }
}
