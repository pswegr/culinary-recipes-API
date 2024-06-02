using System.ComponentModel.DataAnnotations;

namespace CulinaryRecipes.API.Models.Account
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Nick { get; set; }
    }
}
