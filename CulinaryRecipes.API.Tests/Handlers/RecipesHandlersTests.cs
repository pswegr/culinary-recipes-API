using CloudinaryDotNet.Actions;
using CulinaryRecipes.API.Features.Recipes.Commands;
using CulinaryRecipes.API.Features.Recipes.Handlers;
using CulinaryRecipes.API.Features.Recipes.Queries;
using CulinaryRecipes.API.Features.Recipes.Results;
using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Repositories;
using CulinaryRecipes.API.Services.Interfaces;
using CulinaryRecipes.API.UnitOfWork;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Text.Json;
using Xunit;

namespace CulinaryRecipes.API.Tests.Handlers
{
    public class RecipesHandlersTests
    {
        [Fact]
        public async Task GetRecipesQueryHandler_ForwardsArguments()
        {
            var repo = new FakeRecipesRepository
            {
                GetFilteredReturn = new List<Recipes> { CreateRecipe("1") }
            };
            var handler = new GetRecipesQueryHandler(new FakeUnitOfWork(repo));

            var result = await handler.Handle(new GetRecipesQuery(new[] { "tag" }, "cat", "text", true, "nick"), default);

            Assert.Single(result);
            Assert.Equal("1", result[0].id);
            Assert.True(repo.LastGetFilteredArgs.HasValue);
            var args = repo.LastGetFilteredArgs.Value;
            Assert.Equal(new[] { "tag" }, args.Tags);
            Assert.Equal("cat", args.Category);
            Assert.True(args.PublishedOnly == true);
            Assert.Equal("nick", args.UserNick);
            Assert.Equal("text", args.Content);
        }

        [Fact]
        public async Task GetFavoritesQueryHandler_ForwardsArguments()
        {
            var repo = new FakeRecipesRepository
            {
                GetFavoritesReturn = new List<Recipes> { CreateRecipe("2") }
            };
            var handler = new GetFavoritesQueryHandler(new FakeUnitOfWork(repo));

            var result = await handler.Handle(new GetFavoritesQuery("user", new[] { "tag" }, "cat", "text", true), default);

            Assert.Single(result);
            Assert.Equal("2", result[0].id);
            Assert.True(repo.LastGetFavoritesArgs.HasValue);
            var args = repo.LastGetFavoritesArgs.Value;
            Assert.Equal("user", args.UserId);
            Assert.Equal(new[] { "tag" }, args.Tags);
            Assert.Equal("cat", args.Category);
            Assert.Equal("text", args.Content);
            Assert.True(args.PublishedOnly == true);
        }

        [Fact]
        public async Task GetRecipeByIdQueryHandler_ReturnsRecipe()
        {
            var repo = new FakeRecipesRepository
            {
                GetByIdReturn = CreateRecipe("recipe-id")
            };
            var handler = new GetRecipeByIdQueryHandler(new FakeUnitOfWork(repo));

            var result = await handler.Handle(new GetRecipeByIdQuery("recipe-id"), default);

            Assert.NotNull(result);
            Assert.Equal("recipe-id", result!.id);
            Assert.Equal("recipe-id", repo.LastGetByIdId);
        }

        [Fact]
        public async Task GetCategoriesQueryHandler_ReturnsCategories()
        {
            var repo = new FakeRecipesRepository
            {
                CategoriesReturn = new List<string> { "cat1", "cat2" }
            };
            var handler = new GetCategoriesQueryHandler(new FakeUnitOfWork(repo));

            var result = await handler.Handle(new GetCategoriesQuery("cat"), default);

            Assert.Equal(2, result.Count);
            Assert.Equal("cat", repo.LastCategoriesSearchText);
        }

        [Fact]
        public async Task GetTagsQueryHandler_ReturnsTags()
        {
            var repo = new FakeRecipesRepository
            {
                TagsReturn = new List<string> { "tag1", "tag2" }
            };
            var handler = new GetTagsQueryHandler(new FakeUnitOfWork(repo));

            var result = await handler.Handle(new GetTagsQuery(true, "nick"), default);

            Assert.Equal(2, result.Count);
            Assert.Equal(true, repo.LastGetTagsPublishedOnly);
            Assert.Equal("nick", repo.LastGetTagsUserNick);
        }

        [Fact]
        public async Task GetFavoritesTagsQueryHandler_ReturnsTags()
        {
            var repo = new FakeRecipesRepository
            {
                FavoritesTagsReturn = new List<string> { "tag1" }
            };
            var handler = new GetFavoritesTagsQueryHandler(new FakeUnitOfWork(repo));

            var result = await handler.Handle(new GetFavoritesTagsQuery("user", true), default);

            Assert.Single(result);
            Assert.Equal("user", repo.LastGetFavoritesTagsUserId);
            Assert.Equal(true, repo.LastGetFavoritesTagsPublishedOnly);
        }

        [Fact]
        public async Task UpsertRecipeCommandHandler_ReturnsBadRequest_OnInvalidJson()
        {
            var handler = new UpsertRecipeCommandHandler(new FakeUnitOfWork(new FakeRecipesRepository()), new FakePhotoService());

            var result = await handler.Handle(new UpsertRecipeCommand("not-json", null, "nick", false), default);

            Assert.Equal(UpsertRecipeStatus.BadRequest, result.Status);
        }

        [Fact]
        public async Task UpsertRecipeCommandHandler_ReturnsNotFound_WhenUpdatingMissingRecipe()
        {
            var repo = new FakeRecipesRepository { GetByIdReturn = null };
            var handler = new UpsertRecipeCommandHandler(new FakeUnitOfWork(repo), new FakePhotoService());
            var json = JsonSerializer.Serialize(CreateRecipe("1"));

            var result = await handler.Handle(new UpsertRecipeCommand(json, null, "nick", true), default);

            Assert.Equal(UpsertRecipeStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpsertRecipeCommandHandler_ReturnsUnauthorized_WhenNotOwner()
        {
            var repo = new FakeRecipesRepository
            {
                GetByIdReturn = CreateRecipe("1", createdBy: "other")
            };
            var handler = new UpsertRecipeCommandHandler(new FakeUnitOfWork(repo), new FakePhotoService());
            var json = JsonSerializer.Serialize(CreateRecipe("1"));

            var result = await handler.Handle(new UpsertRecipeCommand(json, null, "nick", false), default);

            Assert.Equal(UpsertRecipeStatus.Unauthorized, result.Status);
        }

        [Fact]
        public async Task UpsertRecipeCommandHandler_UpdatesRecipe_WhenOwner()
        {
            var existing = CreateRecipe("1", createdBy: "nick");
            existing.LikedByUsers.Add("user-1");

            var repo = new FakeRecipesRepository
            {
                GetByIdReturn = existing
            };
            var unitOfWork = new FakeUnitOfWork(repo);
            var handler = new UpsertRecipeCommandHandler(unitOfWork, new FakePhotoService());
            var json = JsonSerializer.Serialize(CreateRecipe("1"));

            var result = await handler.Handle(new UpsertRecipeCommand(json, null, "nick", false), default);

            Assert.Equal(UpsertRecipeStatus.Updated, result.Status);
            Assert.Equal(1, repo.ReplaceCallCount);
            Assert.Equal("1", repo.LastReplacedId);
            Assert.NotNull(repo.LastReplaced);
            Assert.Contains("user-1", repo.LastReplaced!.LikedByUsers);
            Assert.Equal("nick", repo.LastReplaced.updatedBy);
            Assert.NotNull(repo.LastReplaced.updatedAt);
            Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        }

        [Fact]
        public async Task UpsertRecipeCommandHandler_CreatesRecipe_WhenNew()
        {
            var repo = new FakeRecipesRepository();
            var unitOfWork = new FakeUnitOfWork(repo);
            var handler = new UpsertRecipeCommandHandler(unitOfWork, new FakePhotoService());
            var json = JsonSerializer.Serialize(CreateRecipe(id: null));

            var result = await handler.Handle(new UpsertRecipeCommand(json, null, "nick", false), default);

            Assert.Equal(UpsertRecipeStatus.Created, result.Status);
            Assert.Equal(1, repo.InsertCallCount);
            Assert.Equal(1, unitOfWork.SaveChangesCallCount);
            Assert.NotNull(result.Recipe);
            Assert.Equal("nick", result.Recipe!.createdBy);
            Assert.True(result.Recipe.isActive);
            Assert.NotNull(result.Recipe.photo);
            Assert.Equal(string.Empty, result.Recipe.photo!.url);
        }

        [Fact]
        public async Task DeleteRecipeCommandHandler_ReturnsFalse_WhenMissing()
        {
            var repo = new FakeRecipesRepository { GetByIdReturn = null };
            var handler = new DeleteRecipeCommandHandler(new FakeUnitOfWork(repo));

            var result = await handler.Handle(new DeleteRecipeCommand("missing"), default);

            Assert.False(result);
            Assert.Equal(0, repo.ReplaceCallCount);
        }

        [Fact]
        public async Task DeleteRecipeCommandHandler_SoftDeletes_WhenFound()
        {
            var repo = new FakeRecipesRepository { GetByIdReturn = CreateRecipe("1") };
            var unitOfWork = new FakeUnitOfWork(repo);
            var handler = new DeleteRecipeCommandHandler(unitOfWork);

            var result = await handler.Handle(new DeleteRecipeCommand("1"), default);

            Assert.True(result);
            Assert.Equal(1, repo.ReplaceCallCount);
            Assert.NotNull(repo.LastReplaced);
            Assert.False(repo.LastReplaced!.isActive);
            Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        }

        [Fact]
        public async Task LikeRecipeToggleCommandHandler_ReturnsRepositoryResult()
        {
            var repo = new FakeRecipesRepository { LikeToggleReturn = true };
            var handler = new LikeRecipeToggleCommandHandler(new FakeUnitOfWork(repo));

            var result = await handler.Handle(new LikeRecipeToggleCommand("recipe", "user"), default);

            Assert.True(result);
            Assert.Equal("recipe", repo.LastLikeToggleRecipeId);
            Assert.Equal("user", repo.LastLikeToggleUserId);
        }

        private static Recipes CreateRecipe(string? id, string? createdBy = null)
        {
            return new Recipes
            {
                id = id,
                title = "title",
                description = "description",
                preparationTime = 1,
                cookingTime = 2,
                servings = 3,
                category = "category",
                imageUrl = "image",
                ingredients = new List<Ingredient>
                {
                    new Ingredient { name = "salt", quantity = "1", unit = "tsp" }
                },
                instructions = new List<string> { "step" },
                tags = new List<string> { "tag" },
                createdBy = createdBy ?? string.Empty,
                updatedBy = string.Empty,
                createdAt = null,
                updatedAt = null,
                photo = new Photo { url = string.Empty, publicId = string.Empty, mainColor = string.Empty },
                published = true,
                isActive = true,
                LikedByUsers = new List<string>()
            };
        }
    }

    internal sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeUnitOfWork(IRecipesRepository recipes)
        {
            Recipes = recipes;
        }

        public IRecipesRepository Recipes { get; }

        public int SaveChangesCallCount { get; private set; }

        public Task SaveChangesAsync()
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }

    internal sealed class FakePhotoService : IPhotoService
    {
        public int UploadCallCount { get; private set; }

        public Task<ImageUploadResult> UploadPhotoAsync(IFormFile file)
        {
            UploadCallCount++;
            return Task.FromResult(new ImageUploadResult());
        }
    }

    internal sealed class FakeRecipesRepository : IRecipesRepository
    {
        public List<Recipes> GetFilteredReturn { get; set; } = new();
        public (string[]? Tags, string? Category, bool? PublishedOnly, string? UserNick, string? Content)? LastGetFilteredArgs { get; private set; }

        public List<Recipes> GetFavoritesReturn { get; set; } = new();
        public (string? UserId, string[]? Tags, string? Category, bool? PublishedOnly, string? Content)? LastGetFavoritesArgs { get; private set; }

        public Recipes? GetByIdReturn { get; set; }
        public string? LastGetByIdId { get; private set; }

        public List<string> CategoriesReturn { get; set; } = new();
        public string? LastCategoriesSearchText { get; private set; }

        public List<string> TagsReturn { get; set; } = new();
        public bool? LastGetTagsPublishedOnly { get; private set; }
        public string? LastGetTagsUserNick { get; private set; }

        public List<string> FavoritesTagsReturn { get; set; } = new();
        public string? LastGetFavoritesTagsUserId { get; private set; }
        public bool? LastGetFavoritesTagsPublishedOnly { get; private set; }

        public bool LikeToggleReturn { get; set; }
        public string? LastLikeToggleRecipeId { get; private set; }
        public string? LastLikeToggleUserId { get; private set; }

        public int InsertCallCount { get; private set; }
        public Recipes? LastInserted { get; private set; }

        public int ReplaceCallCount { get; private set; }
        public string? LastReplacedId { get; private set; }
        public Recipes? LastReplaced { get; private set; }

        public Task<Recipes?> GetByIdAsync(string id)
        {
            LastGetByIdId = id;
            return Task.FromResult(GetByIdReturn);
        }

        public Task<List<Recipes>> GetAsync(FilterDefinition<Recipes> filter)
        {
            return Task.FromResult(new List<Recipes>());
        }

        public Task InsertAsync(Recipes entity)
        {
            InsertCallCount++;
            LastInserted = entity;
            return Task.CompletedTask;
        }

        public Task ReplaceAsync(string id, Recipes entity)
        {
            ReplaceCallCount++;
            LastReplacedId = id;
            LastReplaced = entity;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(FilterDefinition<Recipes> filter, UpdateDefinition<Recipes> update)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id)
        {
            return Task.CompletedTask;
        }

        public Task<List<Recipes>> GetFilteredAsync(string[]? tags = null, string? category = null, bool? publishedOnly = null, string? userNick = "", string? content = "")
        {
            LastGetFilteredArgs = (tags, category, publishedOnly, userNick, content);
            return Task.FromResult(GetFilteredReturn);
        }

        public Task<List<Recipes>> GetFavoritesAsync(string? userId, string[]? tags = null, string? category = null, bool? publishedOnly = null, string? content = "")
        {
            LastGetFavoritesArgs = (userId, tags, category, publishedOnly, content);
            return Task.FromResult(GetFavoritesReturn);
        }

        public List<string> GetCategories(string? searchText)
        {
            LastCategoriesSearchText = searchText;
            return CategoriesReturn;
        }

        public Task<List<string>> GetTagsAsync(bool? publishedOnly = null, string? userNick = "")
        {
            LastGetTagsPublishedOnly = publishedOnly;
            LastGetTagsUserNick = userNick;
            return Task.FromResult(TagsReturn);
        }

        public Task<List<string>> GetFavoritesTagsAsync(string? userId, bool? publishedOnly = null)
        {
            LastGetFavoritesTagsUserId = userId;
            LastGetFavoritesTagsPublishedOnly = publishedOnly;
            return Task.FromResult(FavoritesTagsReturn);
        }

        public Task<bool> LikeRecipeToggleAsync(string recipeId, string userId)
        {
            LastLikeToggleRecipeId = recipeId;
            LastLikeToggleUserId = userId;
            return Task.FromResult(LikeToggleReturn);
        }
    }
}
