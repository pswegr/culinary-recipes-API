using CulinaryRecipes.API.Mediation;

namespace CulinaryRecipes.API.Features.Recipes.Commands
{
    public sealed record LikeRecipeToggleCommand(string RecipeId, string UserId) : IRequest<bool>;
}
