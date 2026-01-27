namespace CulinaryRecipes.API.Data.Interfaces
{
    public interface IMongoCollectionNameResolver
    {
        string GetCollectionName<T>();
    }
}
