using System.ComponentModel.DataAnnotations;

namespace SmartEducation.ViewModels
{
    public class OrgEditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password (optional)")]
        public string? NewPassword { get; set; }
    }
}
