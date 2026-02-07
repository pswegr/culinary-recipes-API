using CulinaryRecipes.API.Data.Messaging.Dao;
using CulinaryRecipes.API.Models.Messaging;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Repositories.Messaging
{
    public class ConversationRepository : MessagingGenericRepository<Conversation>, IConversationRepository
    {
        private readonly IMessagingGenericDao<Conversation> _dao;

        public ConversationRepository(IMessagingGenericDao<Conversation> dao) : base(dao)
        {
            _dao = dao;
        }

        public async Task<Conversation?> GetByParticipantsAsync(string firstUserId, string secondUserId)
        {
            var filter = Builders<Conversation>.Filter.All(x => x.ParticipantUserIds, new[] { firstUserId, secondUserId }) &
                         Builders<Conversation>.Filter.Size(x => x.ParticipantUserIds, 2);

            return await _dao.Collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Conversation>> GetForUserAsync(string userId)
        {
            var filter = Builders<Conversation>.Filter.AnyEq(x => x.ParticipantUserIds, userId);
            return await _dao.Collection.Find(filter)
                .SortByDescending(x => x.LastMessageAt)
                .ThenByDescending(x => x.UpdatedAt)
                .ToListAsync();
        }
    }
}
