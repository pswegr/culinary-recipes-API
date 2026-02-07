using CulinaryRecipes.API.Data.Messaging.Dao;
using CulinaryRecipes.API.Models.Messaging;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Repositories.Messaging
{
    public class MessageRequestRepository : MessagingGenericRepository<MessageRequest>, IMessageRequestRepository
    {
        private readonly IMessagingGenericDao<MessageRequest> _dao;

        public MessageRequestRepository(IMessagingGenericDao<MessageRequest> dao) : base(dao)
        {
            _dao = dao;
        }

        public async Task<MessageRequest?> GetPendingBetweenUsersAsync(string senderUserId, string recipientUserId)
        {
            var directFilter = Builders<MessageRequest>.Filter.Eq(x => x.SenderUserId, senderUserId) &
                               Builders<MessageRequest>.Filter.Eq(x => x.RecipientUserId, recipientUserId) &
                               Builders<MessageRequest>.Filter.Eq(x => x.Status, MessageRequestStatus.Pending);

            var reverseFilter = Builders<MessageRequest>.Filter.Eq(x => x.SenderUserId, recipientUserId) &
                                Builders<MessageRequest>.Filter.Eq(x => x.RecipientUserId, senderUserId) &
                                Builders<MessageRequest>.Filter.Eq(x => x.Status, MessageRequestStatus.Pending);

            var filter = directFilter | reverseFilter;
            return await _dao.Collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<MessageRequest>> GetPendingForRecipientAsync(string recipientUserId)
        {
            var filter = Builders<MessageRequest>.Filter.Eq(x => x.RecipientUserId, recipientUserId) &
                         Builders<MessageRequest>.Filter.Eq(x => x.Status, MessageRequestStatus.Pending);

            return await _dao.Collection.Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
