using CulinaryRecipes.API.Features.Recipes.Commands;
using CulinaryRecipes.API.Features.Recipes.Queries;
using CulinaryRecipes.API.Features.Recipes.Results;
using CulinaryRecipes.API.Mediation;
using CulinaryRecipes.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CulinaryRecipes.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController(IMediator mediator) : ControllerBase
    {
        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        public async Task<List<Recipes>> Get([FromQuery] string[]? tags, [FromQuery] string? category, [FromQuery] string? content) =>
            await mediator.Send(new GetRecipesQuery(tags, category, content));

        [HttpGet("GetAllCreatedByUser")]
        [Authorize]
        public async Task<List<Recipes>> GetAllCreatedByUser([FromQuery] string[]? tags, [FromQuery] string? category, [FromQuery] string? content) =>
            await mediator.Send(new GetRecipesQuery(tags, category, content, UserNick: User.FindFirstValue(ClaimTypes.GivenName)));

        [HttpGet("GetFavorites")]
        [Authorize]
        public async Task<List<Recipes>> GetFavorites([FromQuery] string[]? tags, [FromQuery] string? category, [FromQuery] string? content) =>
            await mediator.Send(new GetFavoritesQuery(User.FindFirstValue(ClaimTypes.NameIdentifier), tags, category, content, PublishedOnly: true));

        [HttpGet]
        public async Task<List<Recipes>> GetPublished([FromQuery] string[]? tags, [FromQuery] string? category, [FromQuery] string? content) =>
            await mediator.Send(new GetRecipesQuery(tags, category, content, PublishedOnly: true));

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Recipes>> Get(string id)
        {
            var recipes = await mediator.Send(new GetRecipeByIdQuery(id));

            if (recipes is null)
            {
                return NotFound();
            }

            return recipes;
        }

        [HttpPost("UpsertWithImage")]
        [Authorize]
        public async Task<IActionResult> Upsert([FromForm] string recipe, IFormFile? photo)
        {
            var userNick = User.FindFirstValue(ClaimTypes.GivenName);
            var isAdmin = User.IsInRole("Admin");
            var result = await mediator.Send(new UpsertRecipeCommand(recipe, photo, userNick, isAdmin));

            return result.Status switch
            {
                UpsertRecipeStatus.BadRequest => BadRequest(),
                UpsertRecipeStatus.NotFound => NotFound(),
                UpsertRecipeStatus.Unauthorized => Unauthorized(),
                UpsertRecipeStatus.Updated => NoContent(),
                UpsertRecipeStatus.Created => CreatedAtAction(nameof(Get), new { id = result.Recipe?.id }, result.Recipe),
                _ => BadRequest()
            };
        }

        [HttpDelete("{id:length(24)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await mediator.Send(new DeleteRecipeCommand(id));
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("Categories")]
        public async Task<ActionResult<List<string>>> GetCategories(string? searchText)
        {
            return await mediator.Send(new GetCategoriesQuery(searchText));
        }

        [HttpGet("Tags")]
        public async Task<ActionResult<List<string>>> GetTags()
        {
            return await mediator.Send(new GetTagsQuery(PublishedOnly: true));
        }

        [HttpGet("TagsForNewRecipe")]
        [Authorize]
        public async Task<ActionResult<List<string>>> GetTagsNewRecipe()
        {
            return await mediator.Send(new GetTagsQuery(PublishedOnly: true));
        }

        [HttpGet("AllTags")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<string>>> GetAllTags()
        {
            return await mediator.Send(new GetTagsQuery());
        }

        [HttpGet("AllTagsCreatedByUser")]
        [Authorize]
        public async Task<ActionResult<List<string>>> GetAllTagsCreatedByUser()
        {
            return await mediator.Send(new GetTagsQuery(UserNick: User.FindFirstValue(ClaimTypes.GivenName)));
        }

        [HttpGet("FavoritesTags")]
        [Authorize]
        public async Task<ActionResult<List<string>>> GetFavoritesTags()
        {
            return await mediator.Send(new GetFavoritesTagsQuery(User.FindFirstValue(ClaimTypes.NameIdentifier), PublishedOnly: true));
        }

        [HttpPost("{id}/likeToggle")]
        [Authorize]
        public async Task<IActionResult> LikeRecipeToggle(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null)
            {
                return BadRequest();
            }
            await mediator.Send(new LikeRecipeToggleCommand(id, userId));
            return Ok();
        }
    }
}
