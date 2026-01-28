using CulinaryRecipes.API.Mediation;

namespace CulinaryRecipes.API.Features.Recipes.Commands
{
    public sealed record DeleteRecipeCommand(string Id) : IRequest<bool>;
}
