using CulinaryRecipes.API.Mediation;

namespace CulinaryRecipes.API.Features.Recipes.Queries
{
    public sealed record GetFavoritesTagsQuery(string? UserId, bool? PublishedOnly = null) : IRequest<List<string>>;
}
