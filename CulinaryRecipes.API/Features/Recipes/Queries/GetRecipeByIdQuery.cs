using CulinaryRecipes.API.Mediation;
using RecipesModel = CulinaryRecipes.API.Models.Recipes;

namespace CulinaryRecipes.API.Features.Recipes.Queries
{
    public sealed record GetRecipeByIdQuery(string Id) : IRequest<RecipesModel?>;
}
