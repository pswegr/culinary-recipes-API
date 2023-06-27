using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly RecipesService _recipesService;

        public RecipesController(RecipesService recipesService) =>
            _recipesService = recipesService;

        [HttpGet]
        public async Task<List<Recipes>> Get() =>
            await _recipesService.GetAsync();

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

        [HttpPost]
        public async Task<IActionResult> Post(Recipes newRecipes)
        {
            await _recipesService.CreateAsync(newRecipes);

            return CreatedAtAction(nameof(Get), new { id = newRecipes.id }, newRecipes);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Recipes updatedRecipes)
        {
            var recipes = await _recipesService.GetAsync(id);

            if (recipes is null)
            {
                return NotFound();
            }

            updatedRecipes.id = recipes.id;

            await _recipesService.UpdateAsync(id, updatedRecipes);

            return NoContent();
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
    }
}
