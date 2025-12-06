using CloudinaryDotNet.Actions;
using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CulinaryRecipes.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController(IRecipesService recipesService, IPhotoService photoService) : ControllerBase
    {
        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        public async Task<List<Recipes>> Get([FromQuery] string[]? tags, [FromQuery] string? category, [FromQuery] string? content) =>
            await recipesService.GetAsync(tags: tags, category: category, content: content);

        [HttpGet("GetAllCreatedByUser")]
        [Authorize]
        public async Task<List<Recipes>> GetAllCreatedByUser([FromQuery] string[]? tags, [FromQuery] string? category,  [FromQuery] string? content) =>
            await recipesService.GetAsync(tags: tags, category: category, userNick: User.FindFirstValue(ClaimTypes.GivenName), content: content);

        [HttpGet("GetFavorites")]
        [Authorize]
        public async Task<List<Recipes>> GetFavorites([FromQuery] string[]? tags, [FromQuery] string? category, [FromQuery] string? content) =>
          await recipesService.GetFavoritesAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), tags, category, publishedOnly: true, content: content);

        [HttpGet]
        public async Task<List<Recipes>> GetPublished([FromQuery] string[]? tags, [FromQuery] string? category, [FromQuery] string? content) =>
          await recipesService.GetAsync(tags, category, publishedOnly: true, content: content);

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Recipes>> Get(string id)
        {
            var recipes = await recipesService.GetAsync(id);

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
            try
            {
                var userNick = User.FindFirstValue(ClaimTypes.GivenName);
                var isAdmin = User.IsInRole("Admin");
                Recipes? recipeModel =
                    JsonSerializer.Deserialize<Recipes>(recipe);
                var photoUploadresult = new ImageUploadResult();

                if (photo != null)
                {
                    photoUploadresult = await photoService.UploadPhotoAsync(photo);
                }

                if (recipeModel is null)
                    return BadRequest();

                if (!string.IsNullOrEmpty(recipeModel.id))
                {
                    var recipeFromDb = await recipesService.GetAsync(recipeModel.id);

                    if (recipeFromDb is null)
                    {
                        return NotFound();
                    }
                    recipeModel.LikedByUsers = recipeFromDb.LikedByUsers;
                    recipeModel.updatedAt = DateTime.UtcNow;

                    if (recipeFromDb.createdBy == userNick)
                    {
                        recipeModel.updatedBy = userNick;
                        await recipesService.UpdateAsync(recipeModel.id, recipeModel, photoUploadresult, userNick);
                    }else if ( isAdmin)
                    {
                        recipeModel.updatedBy = userNick;
                        await recipesService.UpdateAsync(recipeModel.id, recipeModel, photoUploadresult, userNick);
                    }
                    else
                    {
                        return Unauthorized();
                    }
             
                    return NoContent();
                }
                else
                {
                    recipeModel.createdAt = DateTime.UtcNow;
                    recipeModel.createdBy = userNick;
                    await recipesService.CreateAsync(recipeModel, photoUploadresult, userNick);
                    return CreatedAtAction(nameof(Get), new { id = recipeModel.id }, recipeModel);
                }
            }
            catch
            {
                return BadRequest();
            }
           
           
        }

        [HttpDelete("{id:length(24)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var recipes = await recipesService.GetAsync(id);

            if (recipes is null)
            {
                return NotFound();
            }

            await recipesService.RemoveAsync(id, recipes);

            return NoContent();
        }

        [HttpGet("Categories")]
        public ActionResult<List<string>> GetCategories(string? searchText)
        {
            return recipesService.GetCategories(searchText);
        }

        [HttpGet("Tags")]
        public async Task<ActionResult<List<string>>> GetTags()
        {
            return await recipesService.GetTags(publishedOnly: true);
        }

        [HttpGet("TagsForNewRecipe")]
        [Authorize]
        public async Task<ActionResult<List<string>>> GetTagsNewRecipe()
        {
            return await recipesService.GetTags(publishedOnly: true);
        }

        [HttpGet("AllTags")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<string>>> GetAllTags()
        {
            return await recipesService.GetTags();
        }

        [HttpGet("AllTagsCreatedByUser")]
        [Authorize]
        public async Task<ActionResult<List<string>>> GetAllTagsCreatedByUser()
        {
            return await recipesService.GetTags(userNick: User.FindFirstValue(ClaimTypes.GivenName));
        }

        [HttpGet("FavoritesTags")]
        [Authorize]
        public async Task<ActionResult<List<string>>> GetFavoritesTags()
        {
            return await recipesService.GetFavoritesTags(userId: User.FindFirstValue(ClaimTypes.NameIdentifier), publishedOnly: true);
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
            await recipesService.LikeRecipeToggleAsync(id, userId);
            return Ok();
        }
    }
}
