using CulinaryRecipes.API.Models;
using Moq;

namespace CulinaryRecipes.API.Services.Tests
{
    public class RecipesServiceTests
    {
        private readonly Mock<IRecipesRepository> _mockRepo;
        private readonly RecipesService _service;

        public RecipesServiceTests()
        {
            _mockRepo = new Mock<IRecipesRepository>();
            _service = new RecipesService(_mockRepo.Object);
        }

        [Fact]
        public void GetRecipeById_ShouldReturnRecipe_WhenRecipeExists()
        {
            // Arrange
            var recipeId = 1;
            var recipe = new Recipe { Id = recipeId, Name = "Test Recipe" };
            _mockRepo.Setup(repo => repo.GetRecipeById(recipeId)).Returns(recipe);

            // Act
            var result = _service.GetRecipeById(recipeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(recipeId, result.Id);
        }

        [Fact]
        public void GetAllRecipes_ShouldReturnAllRecipes()
        {
            // Arrange
            var recipes = new List<Recipe>
            {
            new Recipe { Id = 1, Name = "Recipe 1" },
            new Recipe { Id = 2, Name = "Recipe 2" }
        };
            _mockRepo.Setup(repo => repo.GetAllRecipes()).Returns(recipes);

            // Act
            var result = _service.GetAllRecipes();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void AddRecipe_ShouldAddRecipe()
        {
            // Arrange
            var recipe = new Recipe { Id = 1, Name = "New Recipe" };
            _mockRepo.Setup(repo => repo.AddRecipe(recipe)).Verifiable();

            // Act
            _service.AddRecipe(recipe);

            // Assert
            _mockRepo.Verify(repo => repo.AddRecipe(recipe), Times.Once);
        }

        [Fact]
        public void DeleteRecipe_ShouldDeleteRecipe()
        {
            // Arrange
            var recipeId = 1;
            _mockRepo.Setup(repo => repo.DeleteRecipe(recipeId)).Verifiable();

            // Act
            _service.DeleteRecipe(recipeId);

            // Assert
            _mockRepo.Verify(repo => repo.DeleteRecipe(recipeId), Times.Once);
        }
    }
}
