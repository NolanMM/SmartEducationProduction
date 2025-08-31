using System.ComponentModel.DataAnnotations;

namespace SmartEducation.ViewModels
{
    public class CreateOrganizationViewModel
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
    }
}
