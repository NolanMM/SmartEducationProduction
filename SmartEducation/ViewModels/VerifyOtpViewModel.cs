using System.ComponentModel.DataAnnotations;

namespace SmartEducation.ViewModels
{
    public class VerifyOtpViewModel
    {
        [Required(ErrorMessage = "Please enter the OTP code.")]
        [Display(Name = "Verification Code")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The verification code must be 6 characters long.")]
        public string? OtpCode { get; set; }
        public string? Email { get; set; }
    }
}
