using CulinaryRecipes.API.Data.Messaging.Interfaces;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Data.Messaging.Dao
{
    public class MessagingMongoGenericDao<T> : IMessagingGenericDao<T>
    {
        public MessagingMongoGenericDao(IMessagingMongoDbContext context)
        {
            Collection = context.GetCollection<T>();
        }

        public IMongoCollection<T> Collection { get; }

        public async Task<List<T>> FindAsync(FilterDefinition<T> filter)
        {
            return await Collection.Find(filter).ToListAsync();
        }

        public async Task<T?> FindOneAsync(FilterDefinition<T> filter)
        {
            return await Collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task InsertAsync(T entity)
        {
            await Collection.InsertOneAsync(entity);
        }

        public async Task ReplaceAsync(FilterDefinition<T> filter, T entity)
        {
            await Collection.ReplaceOneAsync(filter, entity);
        }

        public async Task UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            await Collection.UpdateOneAsync(filter, update);
        }

        public async Task DeleteAsync(FilterDefinition<T> filter)
        {
            await Collection.DeleteOneAsync(filter);
        }
    }
}
