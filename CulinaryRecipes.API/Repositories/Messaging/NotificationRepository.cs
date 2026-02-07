using CulinaryRecipes.API.Data.Messaging.Dao;
using CulinaryRecipes.API.Models.Messaging;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Repositories.Messaging
{
    public class NotificationRepository : MessagingGenericRepository<Notification>, INotificationRepository
    {
        private readonly IMessagingGenericDao<Notification> _dao;

        public NotificationRepository(IMessagingGenericDao<Notification> dao) : base(dao)
        {
            _dao = dao;
        }

        public async Task<List<Notification>> GetForRecipientAsync(string recipientUserId, bool unreadOnly, int take)
        {
            var filter = Builders<Notification>.Filter.Eq(x => x.RecipientUserId, recipientUserId);
            if (unreadOnly)
            {
                filter &= Builders<Notification>.Filter.Eq(x => x.IsRead, false);
            }

            return await _dao.Collection.Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .Limit(take)
                .ToListAsync();
        }

        public async Task<long> GetUnreadCountAsync(string recipientUserId)
        {
            var filter = Builders<Notification>.Filter.Eq(x => x.RecipientUserId, recipientUserId) &
                         Builders<Notification>.Filter.Eq(x => x.IsRead, false);

            return await _dao.Collection.CountDocumentsAsync(filter);
        }
    }
}
