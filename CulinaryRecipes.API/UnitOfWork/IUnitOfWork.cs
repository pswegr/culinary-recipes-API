using CulinaryRecipes.API.Repositories;

namespace CulinaryRecipes.API.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRecipesRepository Recipes { get; }
        Task SaveChangesAsync();
    }
}
