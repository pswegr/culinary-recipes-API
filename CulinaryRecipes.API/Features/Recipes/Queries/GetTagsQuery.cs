using CulinaryRecipes.API.Mediation;

namespace CulinaryRecipes.API.Features.Recipes.Queries
{
    public sealed record GetTagsQuery(bool? PublishedOnly = null, string? UserNick = "") : IRequest<List<string>>;
}
