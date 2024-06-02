namespace CulinaryRecipes.API.Models.Identity
{
    public class IdentityDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UsersCollectionName { get; set; } = null!;
        public string RolesCollectionName { get; set; } = null!;
    }
}
