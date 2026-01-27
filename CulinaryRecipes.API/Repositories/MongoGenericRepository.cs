using CulinaryRecipes.API.Data.Dao;
using CulinaryRecipes.API.Models.Interfaces;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Repositories
{
    public class MongoGenericRepository<T> : IGenericRepository<T> where T : IEntity
    {
        private readonly IGenericDao<T> _dao;

        public MongoGenericRepository(IGenericDao<T> dao)
        {
            _dao = dao;
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq(entity => entity.id, id);
            return await _dao.FindOneAsync(filter);
        }

        public async Task<List<T>> GetAsync(FilterDefinition<T> filter)
        {
            return await _dao.FindAsync(filter);
        }

        public async Task InsertAsync(T entity)
        {
            await _dao.InsertAsync(entity);
        }

        public async Task ReplaceAsync(string id, T entity)
        {
            var filter = Builders<T>.Filter.Eq(item => item.id, id);
            await _dao.ReplaceAsync(filter, entity);
        }

        public async Task UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            await _dao.UpdateAsync(filter, update);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq(item => item.id, id);
            await _dao.DeleteAsync(filter);
        }
    }
}
