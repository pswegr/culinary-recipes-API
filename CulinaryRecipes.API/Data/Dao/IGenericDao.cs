using MongoDB.Driver;

namespace CulinaryRecipes.API.Data.Dao
{
    public interface IGenericDao<T>
    {
        IMongoCollection<T> Collection { get; }
        Task<List<T>> FindAsync(FilterDefinition<T> filter);
        Task<T?> FindOneAsync(FilterDefinition<T> filter);
        Task InsertAsync(T entity);
        Task ReplaceAsync(FilterDefinition<T> filter, T entity);
        Task UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update);
        Task DeleteAsync(FilterDefinition<T> filter);
    }
}
