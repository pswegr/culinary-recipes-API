using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Services;
using CulinaryRecipes.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using System.Text.Json;

namespace CulinaryRecipes.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipesService _recipesService;
        private readonly IPhotoService _photoService;

        public RecipesController(IRecipesService recipesService, IPhotoService photoService)
        {
            _photoService = photoService;
            _recipesService = recipesService;
        }

        [HttpGet("GetAll")]
        public async Task<List<Recipes>> Get() =>
            await _recipesService.GetAsync();

        [HttpGet]
        public async Task<List<Recipes>> GetPublished() =>
          await _recipesService.GetPublishedAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Recipes>> Get(string id)
        {
            var recipes = await _recipesService.GetAsync(id);

            if (recipes is null)
            {
                return NotFound();
            }

            return recipes;
        }

        [HttpPost("UpsertWithImage")]
        public async Task<IActionResult> Upsert([FromForm] string recipe, IFormFile? photo)
        {
            try
            {
                Recipes? recipeModel =
                    JsonSerializer.Deserialize<Recipes>(recipe);

                var photoUploadresult = await _photoService.UploadPhotoAsync(photo);

                if (recipeModel is null)
                    return BadRequest();

                if (!string.IsNullOrEmpty(recipeModel.id))
                {
                    var recipeFromDb = await _recipesService.GetAsync(recipeModel.id);

                    if (recipeFromDb is null)
                    {
                        return NotFound();
                    }

                    await _recipesService.UpdateAsync(recipeModel.id, recipeModel, photoUploadresult);

                    return NoContent();
                }
                else
                {
                    recipeModel.createdAt = DateTime.UtcNow;
                    recipeModel.createdBy = "TODO: Admin development";
                    await _recipesService.CreateAsync(recipeModel, photoUploadresult);
                    return CreatedAtAction(nameof(Get), new { id = recipeModel.id }, recipeModel);
                }
            }
            catch
            {
                return BadRequest();
            }
           
           
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var recipes = await _recipesService.GetAsync(id);

            if (recipes is null)
            {
                return NotFound();
            }

            await _recipesService.RemoveAsync(id);

            return NoContent();
        }

        [HttpGet("Categories")]
        public ActionResult<List<string>> GetCategories(string? searchText)
        {
            return _recipesService.GetCategories(searchText);
        }

        [HttpGet("Tags")]
        public async Task<ActionResult<List<string>>> GetTags()
        {
            return await _recipesService.GetTags();
        }
    }
}
