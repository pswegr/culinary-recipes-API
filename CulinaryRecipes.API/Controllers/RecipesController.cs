﻿using CloudinaryDotNet.Actions;
using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        [Authorize(Roles = "Admin")]
        public async Task<List<Recipes>> Get([FromQuery] string[]? tags, [FromQuery] string? category) =>
            await _recipesService.GetAsync(tags: tags, category: category);

        [HttpGet]
        public async Task<List<Recipes>> GetPublished([FromQuery] string[]? tags, [FromQuery] string? category) =>
          await _recipesService.GetAsync(tags, category, publishedOnly: true);

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
        [Authorize]
        public async Task<IActionResult> Upsert([FromForm] string recipe, IFormFile? photo)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                Recipes? recipeModel =
                    JsonSerializer.Deserialize<Recipes>(recipe);
                var photoUploadresult = new ImageUploadResult();

                if (photo != null)
                {
                    photoUploadresult = await _photoService.UploadPhotoAsync(photo);
                }

                if (recipeModel is null)
                    return BadRequest();

                if (!string.IsNullOrEmpty(recipeModel.id))
                {
                    var recipeFromDb = await _recipesService.GetAsync(recipeModel.id);

                    if (recipeFromDb is null)
                    {
                        return NotFound();
                    }

                    recipeModel.updatedAt = DateTime.UtcNow;

                    if (recipeFromDb.createdBy == userId)
                    {
                        await _recipesService.UpdateAsync(recipeModel.id, recipeModel, photoUploadresult);
                    }else if ( isAdmin)
                    {
                        recipeModel.updatedBy = userId;
                        await _recipesService.UpdateAsync(recipeModel.id, recipeModel, photoUploadresult);
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
                    recipeModel.createdBy = userId ;
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var recipes = await _recipesService.GetAsync(id);

            if (recipes is null)
            {
                return NotFound();
            }

            await _recipesService.RemoveAsync(id, recipes);

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
            return await _recipesService.GetTags(publishedOnly: true);
        }

        [HttpGet("AllTags")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<string>>> GetAllTags()
        {
            return await _recipesService.GetTags();
        }
    }
}
