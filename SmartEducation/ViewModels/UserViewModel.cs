using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SmartEducation.ViewModels
{
    public class UserViewModel
    {
        public string? Id { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public int? OrganizationId { get; set; }

        public IEnumerable<SelectListItem>? Organizations { get; set; }
        public IEnumerable<SelectListItem>? Roles { get; set; }
        public List<string>? SelectedRoles { get; set; }
    }
}
