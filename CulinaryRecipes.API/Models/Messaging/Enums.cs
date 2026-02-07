namespace CulinaryRecipes.API.Models.Messaging
{
    public enum MessageRequestStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    public enum MediaAttachmentType
    {
        Photo,
        Video,
        Link
    }

    public enum NotificationType
    {
        MessageRequest,
        Message,
        Like,
        Action
    }
}
