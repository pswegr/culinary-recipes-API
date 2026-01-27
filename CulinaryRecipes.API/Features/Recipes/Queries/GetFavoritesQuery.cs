using CulinaryRecipes.API.Mediation;
using RecipesModel = CulinaryRecipes.API.Models.Recipes;

namespace CulinaryRecipes.API.Features.Recipes.Queries
{
    public sealed record GetFavoritesQuery(
        string? UserId,
        string[]? Tags = null,
        string? Category = null,
        string? Content = "",
        bool? PublishedOnly = null) : IRequest<List<RecipesModel>>;
}
