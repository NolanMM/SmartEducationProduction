using System.ComponentModel.DataAnnotations;

namespace SmartEducation.ViewModels
{
    public class OrganizationViewModel
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
    }
}
