using System.ComponentModel.DataAnnotations;

namespace CulinaryRecipes.API.Models.Account
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Nickname must be alphanumeric.")]
        public string Nick { get; set; } = string.Empty;
    }
}
