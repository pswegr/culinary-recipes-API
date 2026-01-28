using CulinaryRecipes.API.Models.Interfaces;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Repositories
{
    public interface IGenericRepository<T> where T : IEntity
    {
        Task<T?> GetByIdAsync(string id);
        Task<List<T>> GetAsync(FilterDefinition<T> filter);
        Task InsertAsync(T entity);
        Task ReplaceAsync(string id, T entity);
        Task UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update);
        Task DeleteAsync(string id);
    }
}
