using CulinaryRecipes.API.Data.Messaging.Dao;
using CulinaryRecipes.API.Models.Messaging;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Repositories.Messaging
{
    public class ChatMessageRepository : MessagingGenericRepository<ChatMessage>, IChatMessageRepository
    {
        private readonly IMessagingGenericDao<ChatMessage> _dao;

        public ChatMessageRepository(IMessagingGenericDao<ChatMessage> dao) : base(dao)
        {
            _dao = dao;
        }

        public async Task<List<ChatMessage>> GetForConversationAsync(string conversationId, int skip, int take)
        {
            var filter = Builders<ChatMessage>.Filter.Eq(x => x.ConversationId, conversationId);

            return await _dao.Collection.Find(filter)
                .SortByDescending(x => x.SentAt)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();
        }
    }
}
