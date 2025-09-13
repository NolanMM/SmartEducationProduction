using System.ComponentModel.DataAnnotations;

namespace SmartEducation.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter an email address.")]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }


        [Required(ErrorMessage = "Please enter a password.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword")]
        public string? Password { get; set; }


        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please enter your first name.")]
        [Display(Name = "First Name")]
        public string? First_Name { get; set; }

        [Required(ErrorMessage = "Please enter your last name.")]
        [Display(Name = "Last Name")]
        public string? Last_Name { get; set; }
    }
}
