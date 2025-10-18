using CulinaryRecipes.API.Infrastructure.Interfaces;
using CulinaryRecipes.API.Models;

namespace CulinaryRecipes.API.Data.Repositories;

public class RecipesRepository() : IRecipesRepository
{
    public void AddRecipe(Recipe recipe)
    {
        throw new NotImplementedException();
    }

    public void DeleteRecipe(int recipeId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Recipe> GetAllRecipes()
    {
        throw new NotImplementedException();
    }

    public Recipe GetRecipeById(int recipeId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Recipe> SearchRecipes(string searchTerm)
    {
        throw new NotImplementedException();
    }

    public void UpdateRecipe(Recipe recipe)
    {
        throw new NotImplementedException();
    }
}
