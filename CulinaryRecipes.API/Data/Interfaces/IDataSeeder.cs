namespace CulinaryRecipes.API.Data.Interfaces
{
    public interface IDataSeeder
    {

        Task SeedAdminUserAsync();
        Task SeedRolesAsync();
    }
}
