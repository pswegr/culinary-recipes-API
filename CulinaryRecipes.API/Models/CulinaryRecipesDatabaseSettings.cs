namespace CulinaryRecipes.API.Models
{
    public class CulinaryRecipesDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string RecipesCollectionName { get; set; } = null!;
    }
}
