using System.ComponentModel.DataAnnotations;

namespace CulinaryRecipes.API.Models.Identity
{
    public class AdminUserSettings
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nick { get; set; } = string.Empty;
        public string SecurityStamp { get; set; } = string.Empty;
    }
}
