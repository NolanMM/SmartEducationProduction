using System.ComponentModel.DataAnnotations;

namespace SmartEducation.ViewModels
{
    public class EditOrganizationViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }
    }
}
