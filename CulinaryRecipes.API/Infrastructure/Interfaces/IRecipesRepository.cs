using CulinaryRecipes.API.Models;

namespace CulinaryRecipes.API.Infrastructure.Interfaces;

public interface IRecipesRepository
{
    // Command to add a new recipe
    void AddRecipe(Recipe recipe);

    // Command to update an existing recipe
    void UpdateRecipe(Recipe recipe);

    // Command to delete a recipe
    void DeleteRecipe(int recipeId);

    // Query to get a recipe by its ID
    Recipe GetRecipeById(int recipeId);

    // Query to get all recipes
    IEnumerable<Recipe> GetAllRecipes();

    // Query to search for recipes by a specific criteria
    IEnumerable<Recipe> SearchRecipes(string searchTerm);
}
