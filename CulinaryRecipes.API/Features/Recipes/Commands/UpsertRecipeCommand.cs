using CulinaryRecipes.API.Features.Recipes.Results;
using CulinaryRecipes.API.Mediation;
using Microsoft.AspNetCore.Http;

namespace CulinaryRecipes.API.Features.Recipes.Commands
{
    public sealed record UpsertRecipeCommand(
        string RecipeJson,
        IFormFile? Photo,
        string? UserNick,
        bool IsAdmin) : IRequest<UpsertRecipeResult>;
}
