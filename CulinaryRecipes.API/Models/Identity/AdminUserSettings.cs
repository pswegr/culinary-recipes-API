using System.ComponentModel.DataAnnotations;

namespace CulinaryRecipes.API.Models.Identity
{
    public class AdminUserSettings
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Nick { get; set; } = null!;
        public string SecurityStamp { get; set; }
    }
}
