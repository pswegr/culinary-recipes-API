using CloudinaryDotNet.Actions;
using CulinaryRecipes.API.Features.Recipes.Commands;
using CulinaryRecipes.API.Features.Recipes.Results;
using CulinaryRecipes.API.Mediation;
using CulinaryRecipes.API.Services.Interfaces;
using CulinaryRecipes.API.UnitOfWork;
using RecipesModel = CulinaryRecipes.API.Models.Recipes;
using CulinaryRecipes.API.Models;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CulinaryRecipes.API.Features.Recipes.Handlers
{
    public class UpsertRecipeCommandHandler(IUnitOfWork unitOfWork, IPhotoService photoService) : IRequestHandler<UpsertRecipeCommand, UpsertRecipeResult>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPhotoService _photoService = photoService;

        public async Task<UpsertRecipeResult> Handle(UpsertRecipeCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RecipeJson))
            {
                return UpsertRecipeResult.BadRequest();
            }

            RecipesModel? recipeModel;
            try
            {
                recipeModel = JsonSerializer.Deserialize<RecipesModel>(request.RecipeJson);
            }
            catch (JsonException)
            {
                return UpsertRecipeResult.BadRequest();
            }

            if (recipeModel == null)
            {
                return UpsertRecipeResult.BadRequest();
            }

            var photoUploadResult = new ImageUploadResult();
            if (request.Photo != null)
            {
                photoUploadResult = await _photoService.UploadPhotoAsync(request.Photo);
            }

            if (!string.IsNullOrEmpty(recipeModel.id))
            {
                var recipeFromDb = await _unitOfWork.Recipes.GetByIdAsync(recipeModel.id);
                if (recipeFromDb == null)
                {
                    return UpsertRecipeResult.NotFound();
                }

                recipeModel.LikedByUsers = recipeFromDb.LikedByUsers;
                recipeModel.updatedAt = DateTime.UtcNow;

                if (recipeFromDb.createdBy == request.UserNick || request.IsAdmin)
                {
                    recipeModel.updatedBy = request.UserNick;
                    recipeModel.isActive = true;
                    if (photoUploadResult?.SecureUrl?.AbsoluteUri != null)
                    {
                        recipeModel.photo = BuildPhoto(photoUploadResult);
                    }

                    await _unitOfWork.Recipes.ReplaceAsync(recipeModel.id, recipeModel);
                    await _unitOfWork.SaveChangesAsync();
                    return UpsertRecipeResult.Updated();
                }

                return UpsertRecipeResult.Unauthorized();
            }

            recipeModel.createdAt = DateTime.UtcNow;
            recipeModel.createdBy = request.UserNick;
            recipeModel.isActive = true;

            if (photoUploadResult?.SecureUrl?.AbsoluteUri != null)
            {
                recipeModel.photo = BuildPhoto(photoUploadResult);
            }
            else
            {
                recipeModel.photo = new Photo
                {
                    url = string.Empty,
                    publicId = string.Empty,
                    mainColor = string.Empty
                };
            }

            await _unitOfWork.Recipes.InsertAsync(recipeModel);
            await _unitOfWork.SaveChangesAsync();
            return UpsertRecipeResult.Created(recipeModel);
        }

        private static Photo BuildPhoto(ImageUploadResult imageUploadResult)
        {
            return new Photo
            {
                url = imageUploadResult.SecureUrl.AbsoluteUri,
                publicId = imageUploadResult.PublicId,
                mainColor = imageUploadResult.Colors[0][0]
            };
        }
    }

    public class DeleteRecipeCommandHandler : IRequestHandler<DeleteRecipeCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteRecipeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
        {
            var recipe = await _unitOfWork.Recipes.GetByIdAsync(request.Id);
            if (recipe == null)
            {
                return false;
            }

            recipe.isActive = false;
            await _unitOfWork.Recipes.ReplaceAsync(request.Id, recipe);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }

    public class LikeRecipeToggleCommandHandler : IRequestHandler<LikeRecipeToggleCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public LikeRecipeToggleCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(LikeRecipeToggleCommand request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Recipes.LikeRecipeToggleAsync(request.RecipeId, request.UserId);
        }
    }
}
