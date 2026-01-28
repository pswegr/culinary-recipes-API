using RecipesModel = CulinaryRecipes.API.Models.Recipes;

namespace CulinaryRecipes.API.Features.Recipes.Results
{
    public enum UpsertRecipeStatus
    {
        BadRequest,
        NotFound,
        Unauthorized,
        Updated,
        Created
    }

    public sealed record UpsertRecipeResult(UpsertRecipeStatus Status, RecipesModel? Recipe = null)
    {
        public static UpsertRecipeResult BadRequest() => new(UpsertRecipeStatus.BadRequest);
        public static UpsertRecipeResult NotFound() => new(UpsertRecipeStatus.NotFound);
        public static UpsertRecipeResult Unauthorized() => new(UpsertRecipeStatus.Unauthorized);
        public static UpsertRecipeResult Updated() => new(UpsertRecipeStatus.Updated);
        public static UpsertRecipeResult Created(RecipesModel recipe) => new(UpsertRecipeStatus.Created, recipe);
    }
}
