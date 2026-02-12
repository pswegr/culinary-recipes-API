namespace CulinaryRecipes.API.Models.Messaging.Requests
{
    public class CreateMessageRequestModel
    {
        public string RecipientNick { get; set; } = string.Empty;
        public string RecipientUserId { get; set; } = string.Empty;
    }
}
