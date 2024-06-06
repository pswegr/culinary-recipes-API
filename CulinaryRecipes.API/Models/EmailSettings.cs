namespace CulinaryRecipes.API.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
    }
}
