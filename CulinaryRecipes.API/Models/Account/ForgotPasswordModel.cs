using System.ComponentModel.DataAnnotations;

namespace CulinaryRecipes.API.Models.Account
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
