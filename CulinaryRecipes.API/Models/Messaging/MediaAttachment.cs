namespace CulinaryRecipes.API.Models.Messaging
{
    public class MediaAttachment
    {
        public MediaAttachmentType Type { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
    }
}
