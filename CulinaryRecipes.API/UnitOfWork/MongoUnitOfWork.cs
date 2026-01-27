using CulinaryRecipes.API.Repositories;

namespace CulinaryRecipes.API.UnitOfWork
{
    public class MongoUnitOfWork : IUnitOfWork
    {
        public MongoUnitOfWork(IRecipesRepository recipesRepository)
        {
            Recipes = recipesRepository;
        }

        public IRecipesRepository Recipes { get; }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
