using CulinaryRecipes.API.Features.Recipes.Queries;
using CulinaryRecipes.API.Mediation;
using CulinaryRecipes.API.UnitOfWork;
using RecipesModel = CulinaryRecipes.API.Models.Recipes;
using System.Threading;
using System.Threading.Tasks;

namespace CulinaryRecipes.API.Features.Recipes.Handlers
{
    public class GetRecipesQueryHandler : IRequestHandler<GetRecipesQuery, List<RecipesModel>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetRecipesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<RecipesModel>> Handle(GetRecipesQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Recipes.GetFilteredAsync(
                request.Tags,
                request.Category,
                request.PublishedOnly,
                request.UserNick,
                request.Content);
        }
    }

    public class GetFavoritesQueryHandler : IRequestHandler<GetFavoritesQuery, List<RecipesModel>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetFavoritesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<RecipesModel>> Handle(GetFavoritesQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Recipes.GetFavoritesAsync(
                request.UserId,
                request.Tags,
                request.Category,
                request.PublishedOnly,
                request.Content);
        }
    }

    public class GetRecipeByIdQueryHandler : IRequestHandler<GetRecipeByIdQuery, RecipesModel?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetRecipeByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RecipesModel?> Handle(GetRecipeByIdQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Recipes.GetByIdAsync(request.Id);
        }
    }

    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoriesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<string>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = _unitOfWork.Recipes.GetCategories(request.SearchText);
            return Task.FromResult(categories);
        }
    }

    public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, List<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTagsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<string>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Recipes.GetTagsAsync(request.PublishedOnly, request.UserNick);
        }
    }

    public class GetFavoritesTagsQueryHandler : IRequestHandler<GetFavoritesTagsQuery, List<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetFavoritesTagsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<string>> Handle(GetFavoritesTagsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Recipes.GetFavoritesTagsAsync(request.UserId, request.PublishedOnly);
        }
    }
}
