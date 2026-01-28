namespace CulinaryRecipes.API.Models
{
    public class Image
    {
        public string? Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }
}
