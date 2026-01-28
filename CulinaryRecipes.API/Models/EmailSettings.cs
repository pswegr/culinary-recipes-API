namespace CulinaryRecipes.API.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
    }
}
