using CulinaryRecipes.API.Mediation;

namespace CulinaryRecipes.API.Features.Recipes.Queries
{
    public sealed record GetCategoriesQuery(string? SearchText) : IRequest<List<string>>;
}
