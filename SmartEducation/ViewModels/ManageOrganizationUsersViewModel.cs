using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Entities;
using System.ComponentModel.DataAnnotations;

namespace SmartEducation.ViewModels
{
    public class ManageOrganizationUsersViewModel
    {
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public List<User> Members { get; set; }
        public SelectList UsersNotInOrg { get; set; }
        public string UserIdToAdd { get; set; }

        // Properties to track the organization admin
        public string OrganizationAdminId { get; set; }
        public string OrganizationAdminEmail { get; set; }

        [EmailAddress]
        [Display(Name = "New User Email")]
        public string? NewUserEmail { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? NewUserPassword { get; set; }

    }
}
