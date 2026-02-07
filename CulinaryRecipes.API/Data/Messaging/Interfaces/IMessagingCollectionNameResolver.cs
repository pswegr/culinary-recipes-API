namespace CulinaryRecipes.API.Data.Messaging.Interfaces
{
    public interface IMessagingCollectionNameResolver
    {
        string GetCollectionName<T>();
    }
}
