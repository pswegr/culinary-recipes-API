using CulinaryRecipes.API.Models.Messaging;

namespace CulinaryRecipes.API.Repositories.Messaging
{
    public interface IConversationRepository : IMessagingGenericRepository<Conversation>
    {
        Task<Conversation?> GetByParticipantsAsync(string firstUserId, string secondUserId);
        Task<List<Conversation>> GetForUserAsync(string userId);
    }
}
