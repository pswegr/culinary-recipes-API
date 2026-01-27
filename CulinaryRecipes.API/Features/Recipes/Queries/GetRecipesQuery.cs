using CulinaryRecipes.API.Mediation;
using RecipesModel = CulinaryRecipes.API.Models.Recipes;

namespace CulinaryRecipes.API.Features.Recipes.Queries
{
    public sealed record GetRecipesQuery(
        string[]? Tags = null,
        string? Category = null,
        string? Content = "",
        bool? PublishedOnly = null,
        string? UserNick = "") : IRequest<List<RecipesModel>>;
}
